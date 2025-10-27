# MetroAts 地下鉄系ATC/Sプラグイン  

MetroAtsはBveEXを利用して以下の機能を実現しています：

- BVEが既に持つ軌道回路ベースの保安装置における他閉塞情報の連続受信を実現
- 各鉄道事業者の保安装置プラグインをモジュール化
- プラグイン間の通信を簡素化
- 既存保安装置の動作挙動の改良
- 最近導入された新たな保安装置の追加（開発中）
- BVEのハンドル位置表示を活用し、キーおよび保安装置選択スイッチの位置を表示

使用方法についてはWikiを参照されたい。
[Wiki Page](https://github.com/winup-zhou/MetroAts/wiki)

## Latest Dev Build
[![MSBuild](https://github.com/winup-zhou/MetroAts/actions/workflows/build.yml/badge.svg)](https://github.com/winup-zhou/MetroAts/actions/workflows/build.yml)

## Latest Release
[![GitHub Release](https://img.shields.io/github/v/release/winup-zhou/MetroAts)](https://github.com/winup-zhou/MetroAts/releases/latest)

## License
[The MIT License](LICENSE)

## Dependencies
### [BveEX](https://github.com/automatic9045/BveEX) (PolyForm Noncommercial License 1.0.0)

Copyright (c) 2022 automatic9045

### [TGMT-CBTC](https://github.com/zbx1425/TGMT-CBTC) (MIT)

Copyright (c) 2021 zbx1425

## 連絡先:
Email: 3166832341@qq.com  
Twitter: @wup99925510  

## 現在の開発状況
### コアプラグイン
- [x] 保安装置選択スイッチ
- [x] マスコンキー
### 保安装置
#### 東武
- [x] TSP-ATS
- [x] T-DATC
- [ ] ~~TC-ATS~~ __暫定計画__
#### 西武
- [x] 旧CS-ATC
- [x] 西武ATS
#### メトロ·東葉
- [x] 新CS-ATC
- [x] WS-ATC
- [ ] ~~CS-DATC~~ __暫定計画__
- [ ] ~~ATP~~ __暫定計画__
#### 東急
- [x] ATC-P
- [x] 東急ATS
#### 相鉄·JR
- [x] ATS-P
- [x] ATS-SN
#### 小田急
- [ ] OM-ATS
- [ ] D-ATS-P
### その他
- [x] メトロ総合プラグインとの互換性
- [ ] ATO/TASCプラグインとの互換性

その他の機能については検討中...
