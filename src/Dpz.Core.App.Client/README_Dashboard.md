# 仪表板首页实现说明

## 概述

已成功实现深色主题的仪表板首页，完全基于 Avalonia 12 框架，使用 MVVM 架构和依赖注入。

## 实现的功能模块

### 1. 界面布局

#### 左侧边栏
- Logo 区域
- 13 个功能模块入口：
  - 📄 开箱模目（文章管理）
  - 👤 闪箱模目（账号管理）
  - 🎵 提箱模目（音乐管理）
  - 🎤 毒音模目（录音管理）
  - 💻 望碎箱模目（源代码管理）
  - 💬 吉载模目（评论管理）
  - 📺 士盈模目（弹幕管理）
  - 📋 上盈模目（动态页面管理）
  - ✉️ 研邮包（碎碎念管理）
  - 🖼️ 图许模目（图片管理）
  - ⚙️ 手鱼模目（系统管理）
  - ⏱️ 草程析研目（时间轴管理）
  - 🎬 程箱模目（视频管理）

#### 顶部栏
- 页面标题
- 通知按钮
- 设置按钮
- 用户信息（Admin）

#### 主内容区域

**第一行：统计卡片（4 个）**
- 每个卡片显示图标、标题和数值
- 使用不同的蓝色/紫色渐变背景
- 数值使用随机模拟数据

**第二行：图表区域**
- **生意统计图表**（2/3 宽度）
  - 折线图占位符
  - 支持 7天、30天、90天 切换（UI）
  - 显示两条数据线（8日、98日）
  
- **杂鱼模目饼图**（1/3 宽度）
  - 环形图占位符
  - 存储模块列表：
    - 图币（35.6 GB）
    - 挂鸣（47.2 GB）
    - 短图（12.8 GB）
    - 废币（8.4 GB）

**第三行：列表区域（3 列）**
- **租赁作价**（左列）
  - 用户头像、名称、描述
  - 评分信息
  
- **软硬银行句**（中列）
  - 图标、标题、次数
  - 3 个条目
  
- **软硬解密 + 操作按钮**（右列）
  - 软硬解密列表（2 个条目）
  - 杂鱼解字操作按钮区域（3 个按钮）

### 2. 技术实现

#### 数据模型 ([DashboardModels.cs](Models/DashboardModels.cs))
- `StatCardModel` - 统计卡片模型
- `ChartDataPoint` - 图表数据点模型
- `StorageModuleData` - 存储模块数据模型
- `RentalPriceInfo` - 租赁作价信息模型
- `SoftwareItemInfo` - 软硬件项目信息模型

#### ViewModel ([MainWindowViewModel.cs](ViewModels/MainWindowViewModel.cs))
- 使用 `ObservableCollection` 管理所有数据
- 在构造函数中初始化随机模拟数据
- 集成 `ILogger` 进行日志记录
- 完全支持依赖注入

#### 视图 ([MainWindow.axaml](Views/MainWindow.axaml))
- 响应式布局设计
- 使用 Grid、StackPanel、ScrollViewer 等布局控件
- ItemsControl 绑定集合数据
- 深色主题配色

#### 样式 ([DashboardStyles.axaml](Styles/DashboardStyles.axaml))
- 统一的深色主题颜色方案
- 自定义卡片样式（StatCard、ContentCard）
- 按钮样式（MenuItem、ActionButton）
- 文本样式（CardTitle、StatValue、StatLabel）

### 3. 主题配色

```
主背景：#0F172A（深蓝黑）
卡片背景：#1E293B（深蓝灰）
主蓝色：#3B82F6
主紫色：#8B5CF6
强调橙色：#F59E0B
主要文本：#F1F5F9（浅灰白）
次要文本：#94A3B8（中灰）
边框颜色：#334155
```

### 4. 模拟数据特性

所有数据都是随机生成的模拟数据：
- 统计数值：使用 `Random.Next()` 生成
- 用户信息：预设的示例数据
- 存储大小：固定的示例值
- 图表数据点：随机生成的数值

## 使用方式

### 运行应用
```bash
cd c:\Users\pengq\Documents\project\Dpz.Core.App\src\Dpz.Core.App.Client
dotnet run
```

### 编译应用
```bash
dotnet build
```

## 未来扩展

1. **图表集成**
   - 集成 LiveCharts、OxyPlot 或 ScottPlot
   - 实现真实的折线图和环形图

2. **数据绑定**
   - 连接真实的后端 API
   - 使用 Service 层获取实际数据

3. **交互功能**
   - 侧边栏菜单导航
   - 按钮点击事件处理
   - 数据刷新机制

4. **动画效果**
   - 页面切换动画
   - 数据加载动画
   - 悬停效果

5. **响应式优化**
   - 不同窗口大小的自适应
   - 支持最小化布局

## 项目结构

```
Dpz.Core.App.Client/
├── Models/
│   └── DashboardModels.cs          # 数据模型
├── ViewModels/
│   ├── MainWindowViewModel.cs      # 主窗口 ViewModel
│   └── ExampleViewModel.cs         # 示例 ViewModel
├── Views/
│   ├── MainWindow.axaml            # 主窗口视图
│   └── MainWindow.axaml.cs         # 主窗口代码后台
├── Styles/
│   └── DashboardStyles.axaml       # 仪表板样式
├── App.axaml                        # 应用程序资源
├── Program.cs                       # 程序入口
└── ServiceCollectionExtensions.cs  # 服务注册
```

## 依赖项

- Avalonia 12.0.1
- Avalonia.Desktop 12.0.1
- Avalonia.Themes.Fluent 12.0.1
- Microsoft.Extensions.DependencyInjection 10.0.0
- Serilog 4.2.0

## 编译状态

✅ 编译成功
- 无错误
- 无警告（仅代码格式化建议）
