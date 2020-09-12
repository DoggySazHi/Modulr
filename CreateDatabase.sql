CREATE SCHEMA IF NOT EXISTS Modulr;

CREATE TABLE Stipulatables
(
	id INT AUTO_INCREMENT UNIQUE,
	name VARCHAR(255) NOT NULL,
	testers JSON DEFAULT '[]' NOT NULL,
	required JSON DEFAULT '[]' NOT NULL,
    PRIMARY KEY (id)
);

create user modulr@'192.168.%.%';

GRANT SELECT, INSERT, DELETE, UPDATE ON Modulr.Stipulatables TO modulr@'192.168.%.%';