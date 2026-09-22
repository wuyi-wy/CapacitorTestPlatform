---
name: wpf-patterns
description: WPF开发中的关键模式和坑点 - TabControl复用、动态列、HandyControl样式
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 2ac58104-04a0-4c84-ae61-2f3d0ba7ef83
  modified: 2026-09-19T13:46:58.080Z
---

## WPF TabControl DataGrid 复用问题

TabControl 切换标签页时复用 DataGrid 实例，导致列不刷新。
**How to apply:** 同时使用 `Loaded` 和 `DataContextChanged` 事件处理列生成。`DataContextChanged` 中先取消旧表的 `CollectionChanged` 订阅，再为新表生成列并订阅。

## ClosedXML 合并单元格自适应

`AdjustToContents()` 对合并单元格无效，需要手动计算。
**How to apply:** 用 `AutoFitSheet` 方法遍历每列计算最大字符宽度（中文2倍），行高按 `fontSize × 1.8` 设置。

## HandyControl DataGrid 样式覆盖

HandyControl 主题覆盖 DataGrid 默认样式，自定义样式不生效。
**How to apply:** 自定义样式必须加 `BasedOn="{x:Null}"` 打断继承链。

## BooleanToVisibilityConverter

WPF 内置不支持反向。项目中有 `InverseBoolToVisibilityConverter`（App.xaml 注册）。
**How to apply:** 正向用 `BooleanToVisibilityConverter`，反向用 `InverseBoolToVisibilityConverter`。
