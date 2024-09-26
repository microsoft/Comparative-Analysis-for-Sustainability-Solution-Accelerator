# Test APIs

After successfully [deploying Azure resources](../DeployAzureResources.md), an API service will be available:

![Deployment Success](../images/services/logicappwork02.png)

## Document Manager APIs
| HTTP | Endpoint |
| ---: | :--- |
| POST | `/DocumentManager/RegisterDocument` |
| POST | `/DocumentManager/AskAgainstDocument` |
| GET  | `/DocumentManager/GetAllDocuments` |

## Gap Analysis APIs
| HTTP | Endpoint |
| ---: | :--- |
| POST | `/ESRS/ESRSGapAnalyzerOnQueue` |
| GET  | `/ESRS/GetAllESRSGapAnalysisResults` |

## Benchmark APIs
| HTTP | Endpoint |
| ---: | :--- |
| POST | `/ESRS/ESRSDisclosureBenchmarkOnQueue` |
| GET  | `/ESRS/GetAllESRSBenchmarkResults` |

## Test APIs

Using the [Postman API Platform](https://www.postman.com/), you can leverage the [Postman import files](../../Services/postman/) for testing the API endpoints.