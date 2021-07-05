# 介绍

本桌面应用程序旨在构建最基本功能的GIS客户端软件，使用C#写成，其中包含第三方库，可以创建地理对象、保存、载入等操作

## version 1.0
此版应用程序可以创建多点（MultiPoint）、线（LineString）、多边形（Polygon）以及手绘功能


程序底层对象依据geojson文档进行设计，只包含MultiPoint、LineString以及Polygon，已省去Point，但不涉及MultiLineString、MultiPolygon和GeometryCollection


保存时自动生成geojson，但不保存手绘图像

打开时，选择一个geojson文件，自动加载，加载后即可新增图形并保存

注意：打开geojson文件时，请打开之前由本软件生成好的geojson文件，否则可能出错！


## 还有待加入的功能：

基本功能：

1、平移、缩放画布范围

2、重新渲染后的锯齿修复问题

进阶功能：

1、用于擦除手绘图像的橡皮擦

2、修改已有对象

终极功能：

1、图层管理

2、世界坐标系统