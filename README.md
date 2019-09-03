# bh3tool

  用于从网络修改崩坏三的一些工具。
  
  [游戏3.4]虽然不知道还能不能用，总之更新一下。游戏用了改过Opcode顺序的luac编译了lua脚本，但是依然支持字符串格式的lua，所以用UnluacNET反编译了一下，然后把lua的后缀.bytes改为.txt就可以了。

### EnableTouchServer .Net Core
  .Net Core平台的代理服务端，可以拦截http并修改开启触摸，引用了FiddlerCore拦截http和我自己的UnityAssetBundleTool.dll修改AssetBundle。

### FileDecryptTool
  单独的加解密的工具。包括AES和异或加解密，用于DataVersion.unity3d、Excel_output.unity3d和Setting.unity3d的加解密。

### NetPackageTool
  用于分析NetPacketV1的内容,可以从[il2cppdumper](https://github.com/Perfare/Il2CppDumper)生成的dump.cs提取出cmd_id与类的对应关系，可以从WireShark导出的文件（tcp流导出的HexDump格式）提取出protobuf序列化后的数据并转换为json格式。




###### ***UnityAssetBundleTool.dll是基于[AssetStudio](https://github.com/Perfare/AssetStudio)部分代码写的AssetBundle编辑工具，性能差但是勉强能用。***
