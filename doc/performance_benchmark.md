# Performance Benchmark

## Benchmark Configuration

- Cluster Configuration

  - Broker Node: Standard DS5v2 * 1
  - Backend: Azure Batch (Standard D4Sv3 * 15)
  - Separate client for message throughput test

- Benchmark Metrix

  | Benchmark          | Concurrency | Request Count                      | Request Calculation Time (millisecond) | Request Body Size (byte) | Session Warmup (second) |
  | ------------------ | ----------- | ---------------------------------- | -------------------------------------- | ------------------------ | ----------------------- |
  | Message Throughput | 8           | 100,000                            | 0                                      | 4                        | 20                      |
  | Latency            | 1           | Number of Compute Nodes (15)       | 0                                      | 4                        | 20                      |
  | CPU Efficiency     | 1           | 20 * Number of Compute Nodes (300) | 3,000                                  | 1024                     | 20                      |

## Detailed Result

### Throughput

| SessionId  | IsDurable | SendThroughput(msg/sec) | BrokerThroughputDuration(msg/sec) | OverallThroughput(msg/sec) |
| :--------: | :-------: | :---------------------: | :-------------------------------: | :------------------------: |
| 1573802145 |   FALSE   |        42105.56         |             17374.71              |          17041.05          |
| 1573802176 |   FALSE   |        32653.64         |             16520.37              |          16230.21          |
| 1573802203 |   FALSE   |        34760.32         |             15626.94              |          15373.32          |
| 1573802231 |   FALSE   |        34223.83         |             14614.34              |          14381.93          |
| 1573802259 |   FALSE   |         35350.2         |             15978.04              |          15711.07          |
| 1573802287 |   FALSE   |        35164.43         |             16314.01              |          16040.12          |
| 1573802314 |   FALSE   |        37639.98         |             17763.47              |          17110.79          |
| 1573802341 |   FALSE   |        35359.59         |             17240.47              |          16753.68          |
| 1573802368 |   FALSE   |        33862.85         |             14303.47              |          14091.34          |
| 1573802397 |   FALSE   |        32821.06         |             14000.47              |          13793.11          |

### Latency

| SessionId  | IsDurable | FirstResponseTime(millisec) | WarmFirstResponseTime(millisec) |
| :--------: | :-------: | :-------------------------: | :-----------------------------: |
| 1573805310 |   FALSE   |         21179.5051          |            101.6017             |
| 1573805332 |   FALSE   |         21204.9465          |            105.5158             |
| 1573805353 |   FALSE   |         21141.0614          |             89.4736             |
| 1573805375 |   FALSE   |         21193.8382          |            104.6166             |
| 1573805396 |   FALSE   |         21146.7591          |             94.186              |
| 1573805418 |   FALSE   |         21213.2544          |            109.5435             |
| 1573805439 |   FALSE   |         21263.5465          |             87.1522             |
| 1573805461 |   FALSE   |         21150.4109          |             98.5999             |
| 1573805482 |   FALSE   |         21210.6907          |             95.7905             |
| 1573805504 |   FALSE   |         21184.3898          |             99.4633             |

### CPU Efficiency	

| SessionId  | IsDurable | EfficiencyAfterFirstReqServed | EfficiencyAfterFirstReqServedExcludeSessionEnd |
| ---------- | --------- | ----------------------------- | ---------------------------------------------- |
| 1574050708 | FALSE     | 3.96645169                    | 3.9813                                         |
| 1574050744 | FALSE     | 3.97468498                    | 3.98701                                        |
| 1574050781 | FALSE     | 3.972253189                   | 3.984568                                       |
| 1574050817 | FALSE     | 3.969965353                   | 3.984684                                       |
| 1574050854 | FALSE     | 3.974902166                   | 3.987209                                       |
| 1574050890 | FALSE     | 3.967923218                   | 3.981562                                       |
| 1574050927 | FALSE     | 3.969175405                   | 3.984405                                       |
| 1574050964 | FALSE     | 3.970311834                   | 3.982724                                       |
| 1574051000 | FALSE     | 3.970710505                   | 3.982988                                       |
| 1574051037 | FALSE     | 3.972482338                   | 3.984785                                       |

## Benchmark Your Cluster

- Build TestService from [source code](../perf/TestService).
- Deploy TestService
- Build and test use [TestClient](../perf/TestClient)
  - Sample scripts ([Message Througput](../perf/TestClient/MsgThroughput.ps1), [Latency](../perf/TestClient/FirstResponse.ps1), [CPU Efficiency](../perf/TestClient/CTQ.ps1)) can be found in the project folder.