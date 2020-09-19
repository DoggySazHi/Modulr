CREATE SCHEMA IF NOT EXISTS Modulr;

CREATE TABLE IF NOT EXISTS Modulr.Stipulatables
(
	id INT AUTO_INCREMENT UNIQUE,
	name VARCHAR(255) NOT NULL,
	testers JSON DEFAULT '[]' NOT NULL,
	required JSON DEFAULT '[]' NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS Modulr.Users
(
	id INT AUTO_INCREMENT UNIQUE,
	google_id VARCHAR(64) NOT NULL UNIQUE,
	name VARCHAR(32) NOT NULL UNIQUE,
	username VARCHAR(32) NOT NULL UNIQUE,
	email varchar(128) NOT NULL UNIQUE,

    tests_remaining INT NOT NULL DEFAULT 3,
    tests_timeout TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP(),

	PRIMARY KEY (id)
);

CREATE USER IF NOT EXISTS modulr@'192.168.%.%';

GRANT SELECT, INSERT, DELETE, UPDATE ON Modulr.* TO modulr@'192.168.%.%';