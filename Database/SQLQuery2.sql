-- USE [CarRecoveryDB];
-- GO

IF OBJECT_ID('dbo.CarRecoveries', 'U') IS NOT NULL
    DROP TABLE dbo.CarRecoveries;
GO

CREATE TABLE dbo.CarRecoveries (
    
    NumberPlate VARCHAR(15) NOT NULL PRIMARY KEY,
    
    CarModel VARCHAR(50) NOT NULL,
    
    Issue TEXT,
    Location VARCHAR(255) NOT NULL,
    
    BreakdownTime DATETIME DEFAULT GETDATE(),
    
    Status VARCHAR(20) DEFAULT 'Pending'
);
GO

INSERT INTO dbo.CarRecoveries (NumberPlate, CarModel, Issue, Location, Status)
VALUES
('AB12 CDE', 'Tesla Model 3', 'Flat Battery', 'Hendon Way, London', 'Pending'),
('HN05 NAM', 'Ford Fiesta', 'Engine Overheating', 'M1 Motorway, J4', 'In Progress'),
('BD67 XZZ', 'Mercedes C-Class', 'Puncture - Front Left', 'A406 North Circular', 'Pending'),
('KL19 RTY', 'BMW 3 Series', 'Transmission Failure', 'Brent Cross Shopping Centre', 'In Progress'),
('LP22 VBN', 'Volkswagen Golf', 'Key stuck in ignition', 'High Street, Barnet', 'Completed'),
('SG11 KKK', 'Audi A4', 'Brake Fluid Leak', 'Colindale Avenue', 'Pending'),
('ZA23 QWE', 'Toyota Prius', 'Hybrid System Error', 'Watford Way', 'Completed');
GO
