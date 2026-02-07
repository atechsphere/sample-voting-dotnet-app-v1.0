-- Create database
CREATE DATABASE IF NOT EXISTS VotingDB;
USE VotingDB;

-- Create voting user with proper permissions
CREATE USER IF NOT EXISTS 'votinguser'@'%' IDENTIFIED BY 'votingpassword123';
GRANT ALL PRIVILEGES ON VotingDB.* TO 'votinguser'@'%';
FLUSH PRIVILEGES;

-- Create tables
CREATE TABLE IF NOT EXISTS Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastVotedAt DATETIME NULL,
    INDEX idx_username (Username),
    INDEX idx_email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Votes (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SelectedParty ENUM('Republican', 'Democrat') NOT NULL,
    VotedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_user_voted (UserId, VotedAt),
    INDEX idx_voted_at (VotedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample data
INSERT IGNORE INTO Users (Username, Email, PasswordHash) VALUES
('john_doe', 'john@example.com', '$2a$11$hashedpassword123'),
('jane_smith', 'jane@example.com', '$2a$11$hashedpassword456');

INSERT IGNORE INTO Votes (SelectedParty, UserId, VotedAt) VALUES
('Republican', 1, DATE_SUB(NOW(), INTERVAL 2 DAY)),
('Democrat', 2, DATE_SUB(NOW(), INTERVAL 1 DAY));
