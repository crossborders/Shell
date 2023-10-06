CREATE TABLE permissionCodes (
Codes VARCHAR(64) NOT NULL
);


DECLARE @CreateLimit int = 10; --Oluþturmak istediðimiz kod sayýsý (ihtiyaca göre)
DECLARE @CodeCounter int = 0;
DECLARE @randString VARCHAR(255);
DECLARE @Length int;
DECLARE @CharPool char(62);
DECLARE @PoolLength int;
DECLARE @LoopCount int;


WHILE (@CodeCounter < @CreateLimit) BEGIN
	
			
			SET @Length = 9;
						

			SET @CharPool = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
			SET @PoolLength = Len(@CharPool)

			SET @LoopCount = 0
			SET @randString =''

				WHILE(@LoopCount < 9) BEGIN 
					SELECT @randString = @randString + 
					SUBSTRING(@CharPool , CONVERT(int , RAND() * @PoolLength) + 1 , 1)
					SELECT @LoopCount = @LoopCount + 1
				END

INSERT INTO permissionCodes (Codes)
VALUES (@randString)

SELECT @CodeCounter = @CodeCounter + 1
END

SELECT Codes FROM permissionCodes




	


