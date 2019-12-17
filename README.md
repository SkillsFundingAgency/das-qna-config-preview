# ![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png) Digital Apprenticeships Service

##  QnA Configuration Preview Service

### Developer Setup

#### Requirements

- Install [Visual Studio 2019 Enterprise](https://www.visualstudio.com/downloads/) with these workloads:
    - ASP.NET and web development
    - Azure development
- Install BundlerMinifier extension
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) 
- Install [Azure Storage Explorer](http://storageexplorer.com/)
- Administrator Access

#### Setup

- Create a Configuration table in your (Development) local storage account.
- Add a row to the Configuration table with fields: PartitionKey: LOCAL, RowKey: SFA.DAS.QnA.Config.Preview_1.0 Data: {The QnaApiAuthentication  contents of the QnA Api config json file}.

##### Open the solution

- Open Visual studio as an administrator
- Open the solution
- Set SFA.DAS.QnA.Config.Preview as the startup projects
- Running the solution will launch the site and API in your browser

-or-

- Navigate to src/SFA.DAS.QnA.Config.Preview.Web/
- run `dotnet restore`
- run `dotnet run`
- Open https://localhost:5666