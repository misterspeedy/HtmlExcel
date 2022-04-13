CREATE TABLE
    DownloadHistory
        (
            Id INT IDENTITY NOT NULL,
            Url VARCHAR(MAX) NOT NULL,
            Ip VARCHAR(39) NOT NULL,
            Date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            TableCount INT NOT NULL
        )