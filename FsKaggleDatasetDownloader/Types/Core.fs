namespace FsKaggleDatasetDownloader.Types

module Core =
    type ReportingData =
        { Notes: string
          BytesRead: int64
          TotalBytes: int64
          BytesPerSecond: float }
