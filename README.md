# Modbus 連接狀態統計應用程式

這是一個用於監控和統計 Modbus TCP 連接狀態的 Windows Form 應用程式。它可以記錄連接斷開事件，並提供詳細的統計分析和視覺化報表。

## 功能特點

- Modbus TCP 連接監控
- 自動記錄斷線事件
- 統計分析功能
  - 每小時斷線統計
  - 斷線原因分析
  - 持續時間統計
  - 週環比變化
- 圖表化數據展示
- SQLite 本地數據存儲

## 系統需求

- Windows 作業系統
- .NET Framework 4.5.2 或更高版本
- Visual Studio 2017 或更高版本（用於開發）

## 安裝說明

1. 確保系統已安裝 .NET Framework 4.5.2 或更高版本
2. 下載並解壓縮應用程式
3. 執行 StatisticsConnectStateApp.exe

## 使用說明

1. 主視窗
   - 點擊 "Modbus Connect" 按鈕連接到 Modbus 設備
   - 連接狀態會自動監控並記錄
   
2. 統計視窗
   - 點擊 "統計" 按鈕開啟統計視窗
   - 選擇日期範圍查看統計數據
   - 查看各種統計圖表和指標

## 開發相關

### 專案結構

- MainForm.cs: 主視窗和連接監控
- StatisticsForm.cs: 統計視窗和數據展示
- Models/
  - DatabaseManager.cs: 數據庫管理
  - ConnectionLog.cs: 連接日誌模型
  - StatisticsData.cs: 統計數據模型

### 使用的套件

- NModbus: Modbus 通訊
- EntityFramework: 數據訪問框架
- System.Data.SQLite: SQLite 數據庫存取

## 授權

此專案採用 MIT 授權條款 - 詳見 LICENSE 文件
