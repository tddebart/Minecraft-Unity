# Minecraft in  unity
made by me


## TODO:

- add a rainfall noise map to the world just like the temperature map (https://minecraft.fandom.com/wiki/Biome?file=Biome_Index.PNG)
- maybe make use of compute shaders to generate the world faster



## Times

5 render distance
- optimize 1
  - Data 2807ms
  - Feature 1755ms
  - Mesh 398ms
  - Noise 1747ms
  - Total 5469ms
- optimize 2: removed biomes
  - Data 885ms
  - Feature 405ms
  - Mesh 385ms
  - Noise 537ms
  - Total 2249ms
- optimize 3: biomes back and blocks are now class's and chunks are now made out of chunkSections
  - Data 1931ms
  - Feature 1409ms
  - Mesh 357ms
  - Noise 362ms
  - Total 3833ms
  