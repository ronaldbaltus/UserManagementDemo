CREATE TABLE `User` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `EmailAddress` varchar(255) NOT NULL,
  `HashedPassword` longtext DEFAULT NULL,
  `EmailAddressVerified` tinyint(1) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `IX_User_EmailAddress` (`EmailAddress`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4