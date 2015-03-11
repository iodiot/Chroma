require "rubygems"
require "rmagick"
require "pathname"
include Magick

$maxx = 0
$maxy = 0
$padding = 1

$colors = ['Red', 'Yellow', 'Blue', 'Orange', 'Green', 'Purple']

class Box
   def initialize(name, img, imgn, offX, offY, linkX = -1, linkY = -1, tileAid = false) 
      @name, @img, @imgn = name, img, imgn
      @linkX = linkX
      @linkY = linkY
      @tileAid = tileAid
      @hasNormals = @imgn != nil
      @w = img.columns + $padding * 2 + (tileAid ? 2 : 0)
      @h = img.rows + $padding * 2 + (tileAid ? 2 : 0)
      @a = @w * @h
      @x = @y = 0
      @offX = offX
      @offY = offY
   end 
   attr_reader :w, :h, :a, :name, :img, :imgn, :hasNormals, :x, :y, 
               :linkX, :linkY, :tileAid, :offX, :offY
   attr_writer :x, :y
end
   
class Node
   def initialize(x, y, w, h)
      @box = nil
      @r = nil       #right
      @b = nil       #bottom
      @x, @y, @w, @h = x, y, w, h
   end

   def insert(box)
      if @box == nil 
         if (box.h <= @h) && (box.w <= @w)
            @box = box
            box.x = @x
            box.y = @y
            $maxx = [$maxx, @x + box.w].max
            $maxy = [$maxy, @y + box.h].max
            @r = Node.new(@x + box.w, @y, @w - box.w, box.h) if @w - box.w > 0
            @b = Node.new(@x, @y + box.h, @w, @h - box.h) if @h - box.h > 0
            return true
         else
            return false
         end
      else
         ok = (@r != nil) && (@r.insert(box))
         ok = (@b != nil) && (@b.insert(box)) if !ok
         return ok
      end
   end

   attr_reader :box, :r, :b, :w, :h, :x, :y
end

def drawSprite(box, atlas, natlas)
   if (box.tileAid)
      width = box.img.columns
      height = box.img.rows
      # Top
      sub_img = box.img.dispatch(0, 0, width, 1, "RGBA")
      edge_img = Magick::Image.constitute(width, 1, "RGBA", sub_img)
      atlas.composite!(edge_img, box.x + $padding + 1, box.y + $padding, Magick::OverCompositeOp)
      # Bottom
      sub_img = box.img.dispatch(0, height - 1, width, 1, "RGBA")
      edge_img = Magick::Image.constitute(width, 1, "RGBA", sub_img)
      atlas.composite!(edge_img, box.x + $padding + 1, box.y + $padding + height + 1, Magick::OverCompositeOp)
      # Left
      sub_img = box.img.dispatch(0, 0, 1, height, "RGBA")
      edge_img = Magick::Image.constitute(1, height, "RGBA", sub_img)
      atlas.composite!(edge_img, box.x + $padding, box.y + $padding + 1, Magick::OverCompositeOp)
      # Right
      sub_img = box.img.dispatch(width - 1, 0, 1, height, "RGBA")
      edge_img = Magick::Image.constitute(1, height, "RGBA", sub_img)
      atlas.composite!(edge_img, box.x + $padding + width + 1, box.y + $padding + 1, Magick::OverCompositeOp)
   end
   atlas.composite!(box.img, 
      box.x + $padding + (box.tileAid ? 1 : 0), 
      box.y + $padding + (box.tileAid ? 1 : 0),
      Magick::OverCompositeOp)
   if box.imgn != nil
      natlas.composite!(box.imgn, 
         box.x + $padding + (box.tileAid ? 1 : 0), 
         box.y + $padding + (box.tileAid ? 1 : 0), 
         Magick::OverCompositeOp)
   end
end

imgs = Array.new

File.open("atlas.json", "w") do |map|
   File.open("SpriteNames.cs", "w") do |enum|
      enum.puts("using System;")
      enum.puts("")
      enum.puts("    //-------------------------------\\\\")
      enum.puts("    //    Generated automatically    \\\\")
      enum.puts("    //         Do not modify         \\\\")
      enum.puts("    //-------------------------------\\\\")
      enum.puts("")
      enum.puts("namespace Chroma.Graphics")
      enum.puts("{")
      enum.puts("  public enum SpriteName {")
      enum.puts("")

      lastFilePath = nil
      filesRoot = "_sprites"

      files = Dir["_sprites/**/*.png"] 
      files.each_with_index do |file, fi|

         # Structural comments for enum
         thisFilePath = File.dirname(file).split('/').drop(1)
         if lastFilePath != thisFilePath
            if thisFilePath == []
               enum.puts("")
               enum.puts("    // " + "=" * 45)
            else
               thisFilePath.each_with_index do |dirName, di|
                  if lastFilePath == nil || lastFilePath[di] != dirName
                     case di
                        when 0
                           enum.puts(" " * 48 + "// |") if fi > 0
                           enum.puts("    // #{dirName.upcase} " + "=" * (44 - dirName.length))
                        when 1
                           enum.puts(" " * 48 + "// |")
                           enum.puts("    // #{dirName} " + "-" * (44 - dirName.length))
                        else
                           comment = "    // " + '-' * (di - 2) + dirName
                           comment += " " * (48 - comment.length) + "// |"
                           enum.puts(comment)
                     end
                  end
               end
            end
            lastFilePath = thisFilePath
         end

         name = File.basename(file, '.png').downcase
         img = Image.read(file)[0]
         imgn = nil
         linkX = -1
         linkY = -1
         offX = 0
         offY = 0
         tileAid = false

         npath = File.path(file).split('/').drop(1).unshift('_normals').join('/')
         if File.exist?(npath)
            imgn = Image.read(File.open(npath, 'r'))[0]
         end

         # Link point
         if name[0] == "@"
            name[0] = ''

            linkColor = img.pixel_color(0, 0)
            healColor = img.pixel_color(1, 0)
            img.crop!(0, 1, img.columns, img.rows - 1);
            (0..img.columns).each do |x|
               (0..img.rows).each do |y|
                  pixel = img.pixel_color(x, y)
                  if pixel == linkColor
                     img.pixel_color(x, y, healColor)
                     linkX = x
                     linkY = y
                  end
               end
               break if linkX != -1
            end
         end

         # Tiled border aid
         if name[0] == "#"
            name[0] = ''
            tileAid = true        
         end

         # Trim
         noTrim = false
         if name[0] == "^"
            name[0] = ''
            noTrim = true        
         else
            # Trim X
            img = img.splice(img.columns, 0, 1, 0, "red")
            offX = img.columns
            img.border!(1, 1, "transparent")
            img.trim!
            offX -= img.columns
            img = img.chop(img.columns - 1, 0, 1, 0)
            # Trim Y
            img = img.splice(0, img.rows, 0, 1, "red")
            offY = img.rows
            img.border!(1, 1, "transparent")
            img.trim!
            offY -= img.rows
            img = img.chop(0, img.rows - 1, 0, 1)
            # Trim rest
            img.border!(1, 1, "transparent")
            img.trim!
         end

         # Palette id
         space = name.index(" ")
         if space
            colormap = name[space + 1..-1]
            name = name[0, space]

            divider = colormap.index("|")
            colornum = (colormap[divider + 1..-1]).to_i - 1
            colormap = colormap[0, divider]

            cmap = Image.read(File.open("_sprites/_AUX/color_map_" + colormap + ".png", 'r'))[0]

            for i in 0..5
               cname = name + "/" + $colors[i]
               colored = img.clone
               if i != colornum

                  (0..cmap.columns).each do |x|
                     colored = colored.opaque_channel(cmap.pixel_color(x, colornum), cmap.pixel_color(x, i))
                  end

               end
               b = Box.new(cname, colored, imgn, offX, offY, linkX, linkY, tileAid)
               imgs << b
            end

         else
            b = Box.new(name, img, imgn, offX, offY, linkX, linkY, tileAid)
            imgs << b
         end
         
         # Enum value
         enumLine = "    #{name}" + ((fi == files.size - 1) ? "" : ",")
         enumComment = ""
         enumComment += " has link |" if linkX != -1
         enumComment += " tiled |" if tileAid
         enumComment += " no trim |" if noTrim
         enumComment += " colored |" if space
         enumComment += " |" if enumComment == ""
         enumComment = " " * (50 - enumLine.length - enumComment.length) + "//" + enumComment
         enum.puts(enumLine + enumComment)
      end

      imgs.sort! {|a, b| b.a <=> a.a}

      node = Node.new(0, 0, 1024, 2048)
      imgs.each do |b|
         node.insert(b)
      end

      imgs.sort! {|a, b| a.name <=> b.name}
      atlas = Image.new($maxx, $maxy) { self.background_color = "transparent" }
      natlas = Image.new($maxx, $maxy) { self.background_color = "\#800080" }
      
      map.puts(
   "{
   \t\"image-path\": \"atlas.png\",
   \t\"width\": \"#{$maxx}\",
   \t\"height\": \"#{$maxy}\",
   \t\"sprites\": [")

      imgs.each_with_index do |box, i| 
         drawSprite(box, atlas, natlas)
         map.puts("\t\t{")
         map.puts("\t\t\t\"name\": \"#{box.name}\",")
         map.puts("\t\t\t\"x\": \"#{box.x + $padding + (box.tileAid ? 1 : 0)}\",")
         map.puts("\t\t\t\"y\": \"#{box.y + $padding + (box.tileAid ? 1 : 0)}\",")
         map.puts("\t\t\t\"width\": \"#{box.w - 2 * $padding - (box.tileAid ? 2 : 0)}\",")
         map.puts("\t\t\t\"height\": \"#{box.h - 2 * $padding - (box.tileAid ? 2 : 0)}\",")
         map.puts("\t\t\t\"off-x\": \"#{box.offX}\",")
         map.puts("\t\t\t\"off-y\": \"#{box.offY}\",")
         
         if box.linkX != -1 
            map.puts("\t\t\t\"link\": \"true\",")
            map.puts("\t\t\t\"link-x\": \"#{box.linkX}\",")
            map.puts("\t\t\t\"link-y\": \"#{box.linkY}\",")
         else
            map.puts("\t\t\t\"link\": \"false\",")
         end
         #map.puts("\t\t\t\"normal-map\": \"#{box.hasNormals}\"")
         if i == imgs.size - 1
            map.puts("\t\t}")
         else
            map.puts("\t\t},")
         end
      end

      map.puts("\t]\n}")

      atlas.write('atlas.png')
      #natlas.write('atlas-normals.png')

      enum.puts("    // ---------------------------------------------")
      enum.puts("")
      enum.puts("  }")
      enum.puts("}")
   end
end;