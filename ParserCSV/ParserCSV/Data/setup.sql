CREATE TABLE TaxiTrips (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    tpep_pickup_datetime DATETIME2(7) NOT NULL,
    tpep_dropoff_datetime DATETIME2(7) NOT NULL,
    passenger_count INT,
    trip_distance DECIMAL(10, 2), 
    store_and_fwd_flag VARCHAR(3) NOT NULL, -- 'Yes'/'No' 
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    fare_amount DECIMAL(10, 2),
    tip_amount DECIMAL(10, 2),

    trip_duration_seconds AS DATEDIFF(second, tpep_pickup_datetime, tpep_dropoff_datetime) PERSISTED
);

-- 1. For avg tip_amount by PULocationId (Req 4.1)
--    and for searching by PULocationId (Req 4.4)
--    We INCLUDE tip_amount to avoid table lookups
CREATE INDEX IX_TaxiTrips_PULocationID 
ON TaxiTrips(PULocationID) 
INCLUDE (tip_amount);

-- 2. For top 100 longest fares by distance (Req 4.2)
CREATE INDEX IX_TaxiTrips_trip_distance 
ON TaxiTrips(trip_distance DESC);

-- 3. For top 100 longest fares by time (Req 4.3)
--    Index on our new computed column
CREATE INDEX IX_TaxiTrips_trip_duration_seconds 
ON TaxiTrips(trip_duration_seconds DESC);

-- 4. For deduplication (Req 6)
--    This index also helps speed up uniqueness checks.
--    We will handle duplicates in C# code, but this index ensures data integrity.
CREATE UNIQUE INDEX UQ_TaxiTrips_KeyFields
ON TaxiTrips(tpep_pickup_datetime, tpep_dropoff_datetime, passenger_count)
WITH (IGNORE_DUP_KEY = OFF);