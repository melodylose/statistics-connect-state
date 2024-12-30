# 更新日誌

所有關於此專案的重要更改都會記錄在此文件中。

此專案遵循 [語意化版本](https://semver.org/lang/zh-TW/) 規範。

## [1.0.0] - 2024-12-30

### 新增
- 初始版本發布
- Modbus TCP 連接監控功能
- 連接狀態自動記錄
- SQLite 數據庫整合
- 統計分析功能
  - 每小時斷線統計
  - 斷線原因分析
  - 持續時間統計
  - 週環比變化
- 圖表化數據展示
  - 柱狀圖顯示每小時斷線次數
  - 圓餅圖顯示斷線原因分布

### 技術細節
- 使用 .NET Framework 4.5.2
- 整合 NModbus 套件
- 使用 EntityFramework 和 SQLite
- 實現單例模式的數據庫管理
- 多線程處理連接監控

### 已知問題
- 無

## 版本號說明
- 主版本號：重大功能更新或不相容的 API 修改
- 次版本號：向下相容的功能新增
- 修訂號：向下相容的問題修正