-- Seed Data for Raffle System (Updated Schema)
-- Compatible with RaffleStatus enum: 0=Draft, 1=Active, 2=SalesClosed, 3=Drawing, 4=Completed, 5=Cancelled

-- Clear existing data (optional - uncomment if needed)
-- TRUNCATE TABLE RafflePrizes;
-- TRUNCATE TABLE RaffleImages;
-- DELETE FROM Raffles WHERE Id > 0;

-- Insert sample raffles
INSERT INTO Raffles (Title, ShortDescription, FullDescription, TicketPrice, TotalTickets, TicketsSold, MaxTicketsPerUser, PrimaryImageUrl, SalesStartDate, SalesEndDate, DrawDate, CreatedAt, UpdatedAt, Status, IsFeatured)
SELECT * FROM (
    SELECT 
        'FINAL MATCH: THE ULTIMATE SHOWDOWN' as Title,
        'Your chance to attend the World Cup Final! This exclusive package includes everything you need for the ultimate football experience.' as ShortDescription,
        'Experience the pinnacle of world football! This once-in-a-lifetime package gives you and three guests front-row access to the most watched sporting event on the planet. The World Cup Final VIP experience includes premium seating, exclusive hospitality areas, and memories that will last forever.' as FullDescription,
        50.00 as TicketPrice,
        1000 as TotalTickets,
        0 as TicketsSold,
        10 as MaxTicketsPerUser,
        '/images/raffle-final-match.jpg' as PrimaryImageUrl,
        NOW() as SalesStartDate,
        DATE_ADD(NOW(), INTERVAL 29 DAY) as SalesEndDate,
        DATE_ADD(NOW(), INTERVAL 30 DAY) as DrawDate,
        NOW() as CreatedAt,
        NOW() as UpdatedAt,
        1 as Status,  -- Active
        1 as IsFeatured
) AS tmp
WHERE NOT EXISTS (
    SELECT 1 FROM Raffles WHERE Title = 'FINAL MATCH: THE ULTIMATE SHOWDOWN'
);

INSERT INTO Raffles (Title, ShortDescription, FullDescription, TicketPrice, TotalTickets, TicketsSold, MaxTicketsPerUser, PrimaryImageUrl, SalesStartDate, SalesEndDate, DrawDate, CreatedAt, UpdatedAt, Status, IsFeatured)
SELECT * FROM (
    SELECT 
        'SEMI-FINAL 1: CLASH OF TITANS' as Title,
        'Experience the intensity of the World Cup Semi-Final! Watch the world''s best teams compete for a spot in the final.' as ShortDescription,
        'The Semi-Finals are where legends are made. This VIP package puts you at the heart of the action as two world-class teams battle for their place in history. Feel the tension, hear the roar of the crowd, and witness football at its finest.' as FullDescription,
        35.00 as TicketPrice,
        800 as TotalTickets,
        0 as TicketsSold,
        10 as MaxTicketsPerUser,
        '/images/raffle-semifinal-1.jpg' as PrimaryImageUrl,
        NOW() as SalesStartDate,
        DATE_ADD(NOW(), INTERVAL 24 DAY) as SalesEndDate,
        DATE_ADD(NOW(), INTERVAL 25 DAY) as DrawDate,
        NOW() as CreatedAt,
        NOW() as UpdatedAt,
        1 as Status,  -- Active
        0 as IsFeatured
) AS tmp
WHERE NOT EXISTS (
    SELECT 1 FROM Raffles WHERE Title = 'SEMI-FINAL 1: CLASH OF TITANS'
);

INSERT INTO Raffles (Title, ShortDescription, FullDescription, TicketPrice, TotalTickets, TicketsSold, MaxTicketsPerUser, PrimaryImageUrl, SalesStartDate, SalesEndDate, DrawDate, CreatedAt, UpdatedAt, Status, IsFeatured)
SELECT * FROM (
    SELECT 
        'SEMI-FINAL 2: ROAD TO GLORY' as Title,
        'Be part of football history! Witness an epic Semi-Final match with our all-inclusive VIP package.' as ShortDescription,
        'The road to glory runs through this Semi-Final. Join us for an unforgettable experience as two footballing giants clash for supremacy. This all-inclusive VIP package ensures you experience every moment in style.' as FullDescription,
        35.00 as TicketPrice,
        800 as TotalTickets,
        0 as TicketsSold,
        10 as MaxTicketsPerUser,
        '/images/raffle-semifinal-2.jpg' as PrimaryImageUrl,
        NOW() as SalesStartDate,
        DATE_ADD(NOW(), INTERVAL 24 DAY) as SalesEndDate,
        DATE_ADD(NOW(), INTERVAL 25 DAY) as DrawDate,
        NOW() as CreatedAt,
        NOW() as UpdatedAt,
        1 as Status,  -- Active
        0 as IsFeatured
) AS tmp
WHERE NOT EXISTS (
    SELECT 1 FROM Raffles WHERE Title = 'SEMI-FINAL 2: ROAD TO GLORY'
);

-- Insert prizes for Final Match
INSERT INTO RafflePrizes (RaffleId, Description, Icon, DisplayOrder)
SELECT r.Id, p.Description, p.Icon, p.DisplayOrder
FROM Raffles r
CROSS JOIN (
    SELECT '4 VIP Tickets to the World Cup Final' as Description, 'fas fa-ticket-alt' as Icon, 0 as DisplayOrder
    UNION ALL SELECT 'Premium Hotel (3 nights)', 'fas fa-hotel', 1
    UNION ALL SELECT 'Round-trip Flights for 4', 'fas fa-plane', 2
    UNION ALL SELECT 'VIP Pre-Game Party Access', 'fas fa-glass-cheers', 3
    UNION ALL SELECT 'Exclusive Post-Game Celebration', 'fas fa-star', 4
    UNION ALL SELECT 'Local Transportation', 'fas fa-car', 5
) p
WHERE r.Title = 'FINAL MATCH: THE ULTIMATE SHOWDOWN'
AND NOT EXISTS (
    SELECT 1 FROM RafflePrizes rp WHERE rp.RaffleId = r.Id AND rp.Description = p.Description
);

-- Insert prizes for Semi-Final 1
INSERT INTO RafflePrizes (RaffleId, Description, Icon, DisplayOrder)
SELECT r.Id, p.Description, p.Icon, p.DisplayOrder
FROM Raffles r
CROSS JOIN (
    SELECT '4 VIP Tickets to Semi-Final 1' as Description, 'fas fa-ticket-alt' as Icon, 0 as DisplayOrder
    UNION ALL SELECT 'Premium Hotel (3 nights)', 'fas fa-hotel', 1
    UNION ALL SELECT 'Round-trip Flights for 4', 'fas fa-plane', 2
    UNION ALL SELECT 'VIP Pre-Game Party Access', 'fas fa-glass-cheers', 3
    UNION ALL SELECT 'Local Transportation', 'fas fa-car', 4
) p
WHERE r.Title = 'SEMI-FINAL 1: CLASH OF TITANS'
AND NOT EXISTS (
    SELECT 1 FROM RafflePrizes rp WHERE rp.RaffleId = r.Id AND rp.Description = p.Description
);

-- Insert prizes for Semi-Final 2
INSERT INTO RafflePrizes (RaffleId, Description, Icon, DisplayOrder)
SELECT r.Id, p.Description, p.Icon, p.DisplayOrder
FROM Raffles r
CROSS JOIN (
    SELECT '4 VIP Tickets to Semi-Final 2' as Description, 'fas fa-ticket-alt' as Icon, 0 as DisplayOrder
    UNION ALL SELECT 'Premium Hotel (3 nights)', 'fas fa-hotel', 1
    UNION ALL SELECT 'Round-trip Flights for 4', 'fas fa-plane', 2
    UNION ALL SELECT 'VIP Pre-Game Party Access', 'fas fa-glass-cheers', 3
    UNION ALL SELECT 'Local Transportation', 'fas fa-car', 4
) p
WHERE r.Title = 'SEMI-FINAL 2: ROAD TO GLORY'
AND NOT EXISTS (
    SELECT 1 FROM RafflePrizes rp WHERE rp.RaffleId = r.Id AND rp.Description = p.Description
);

-- Show created raffles with prize count
SELECT 
    r.Id,
    r.Title,
    r.TicketPrice,
    r.TotalTickets,
    r.Status,
    r.IsFeatured,
    r.DrawDate,
    (SELECT COUNT(*) FROM RafflePrizes WHERE RaffleId = r.Id) as PrizeCount
FROM Raffles r
ORDER BY r.IsFeatured DESC, r.DrawDate;
