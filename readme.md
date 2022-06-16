## ClipboardPurifier 剪切板净化工具

![](https://s2.loli.net/2022/06/16/jKcXv3Dkx8FU5au.png)

### Why？
不知你有没有遇到过这种场景：

在一个地方复制一串带格式文本（比如PowerPoint），

![](https://s2.loli.net/2022/06/16/ZAIalSThidWxw3K.png)

在另一个地方粘贴（比如QQ聊天框）

![](https://s2.loli.net/2022/06/16/emk2GUJph1VtDK6.png)

可以看到默认粘贴了图片。

如果想粘贴文字，只能粘贴到记事本再复制一遍。这极其不优雅。

于是开发了这么一个小工具，定时检测剪切板，可以自主选择只保留纯文本或只保留图片。

### How？

本程序使用 C# WPF 开发，基于.NET 5.0框架，win10系统的用户请先[安装框架](https://dotnet.microsoft.com/zh-cn/download/dotnet/5.0/runtime)（如果你不知道框架是什么）

值得一提的是，主界面的两个大按钮其实是重写样式的RadioButton，下面的Toggle是重写样式的CheckBox。重写样式时加入了一些交互动画，因此是很好的学习材料。

还有MVVM模式和数据绑定的转换器，本程序均有用到。

### Thanks

感谢Icon8提供的图标和Material Design提供的配色

特别感谢walterlv大神的[ClipboardViewer项目](https://github.com/walterlv/ClipboardViewer)，为我提供了灵感！（虽然我并没有用大神的代码hhh）