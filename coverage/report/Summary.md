# Summary

|||
|:---|:---|
| Generated on: | 05-Nov-26 - 19:42:13 |
| Coverage date: | 03-Nov-26 - 15:04:29 - 05-Nov-26 - 19:42:10 |
| Parser: | MultiReport (3x Cobertura) |
| Assemblies: | 3 |
| Classes: | 20 |
| Files: | 21 |
| **Line coverage:** | 96.2% (803 of 834) |
| Covered lines: | 803 |
| Uncovered lines: | 31 |
| Coverable lines: | 834 |
| Total lines: | 1178 |
| **Branch coverage:** | 90.3% (112 of 124) |
| Covered branches: | 112 |
| Total branches: | 124 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

# Risk Hotspots

| **Assembly** | **Class** | **Method** | **Crap Score** | **Cyclomatic complexity** |
|:---|:---|:---|---:|---:|
| Microclimate.Core | Microclimate.Core.Services.ComfortAssessor | Assess(...) | 22 | 22 || Microclimate.Core | Microclimate.Core.Services.RuleEvaluator | IsViolated(...) | 18 | 18 || Microclimate.Data | Microclimate.Data.PollingService | PollOnceAsync() | 18 | 18 |
# Coverage

| **Name** | **Covered** | **Uncovered** | **Coverable** | **Total** | **Line coverage** | **Covered** | **Total** | **Branch coverage** |
|:---|---:|---:|---:|---:|---:|---:|---:|---:|
| **Microclimate.Core** | **105** | **5** | **110** | **218** | **95.4%** | **71** | **74** | **95.9%** |
| Microclimate.Core.Models.Alert | 7 | 1 | 8 | 14 | 87.5% | 0 | 0 |  |
| Microclimate.Core.Models.Measurement | 3 | 2 | 5 | 10 | 60% | 0 | 0 |  |
| Microclimate.Core.Models.Room | 4 | 0 | 4 | 9 | 100% | 0 | 0 |  |
| Microclimate.Core.Models.Rule | 5 | 0 | 5 | 12 | 100% | 0 | 0 |  |
| Microclimate.Core.Models.Sensor | 5 | 1 | 6 | 11 | 83.3% | 0 | 0 |  |
| Microclimate.Core.Services.AlertFormatter | 17 | 1 | 18 | 30 | 94.4% | 9 | 10 | 90% |
| Microclimate.Core.Services.ComfortAssessor | 10 | 0 | 10 | 31 | 100% | 21 | 22 | 95.4% |
| Microclimate.Core.Services.Downsampler | 18 | 0 | 18 | 33 | 100% | 9 | 10 | 90% |
| Microclimate.Core.Services.MeasurementFilter | 26 | 0 | 26 | 44 | 100% | 12 | 12 | 100% |
| Microclimate.Core.Services.RuleEvaluator | 10 | 0 | 10 | 24 | 100% | 20 | 20 | 100% |
| **Microclimate.Data** | **671** | **25** | **696** | **913** | **96.4%** | **33** | **40** | **82.5%** |
| Microclimate.Data.AlertRepository | 18 | 0 | 18 | 38 | 100% | 0 | 0 |  |
| Microclimate.Data.DbInitializer | 14 | 0 | 14 | 25 | 100% | 4 | 4 | 100% |
| Microclimate.Data.DesignTimeFactory | 0 | 5 | 5 | 14 | 0% | 0 | 0 |  |
| Microclimate.Data.MeasurementRepository | 22 | 0 | 22 | 41 | 100% | 2 | 2 | 100% |
| Microclimate.Data.MicroclimateDbContext | 35 | 0 | 35 | 51 | 100% | 0 | 0 |  |
| Microclimate.Data.Migrations.InitialCreate | 318 | 12 | 330 | 395 | 96.3% | 0 | 0 |  |
| Microclimate.Data.Migrations.MicroclimateDbContextModelSnapshot | 190 | 0 | 190 | 219 | 100% | 0 | 0 |  |
| Microclimate.Data.PollingService | 61 | 2 | 63 | 95 | 96.8% | 21 | 24 | 87.5% |
| Microclimate.Data.RoomRepository | 13 | 6 | 19 | 35 | 68.4% | 6 | 10 | 60% |
| **Microclimate.Sensors** | **27** | **1** | **28** | **47** | **96.4%** | **8** | **10** | **80%** |
| Microclimate.Sensors.SimulatedSensorSource | 27 | 1 | 28 | 47 | 96.4% | 8 | 10 | 80% |

