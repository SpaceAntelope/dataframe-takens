namespace FsKaggleDatasetDownloader.Types

module Core =
    type ReportingData =
        { DestinationPath: string
          BytesRead: int64
          TotalBytes: int64 }
