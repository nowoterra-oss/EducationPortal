-- Soft delete counselor permissions from database
UPDATE Permissions SET IsDeleted=1, IsActive=0 WHERE Code LIKE 'counselor.%';

-- Verify counselor permissions are deleted
SELECT Id, Code, IsDeleted, IsActive FROM Permissions WHERE Code LIKE 'counselor.%';
