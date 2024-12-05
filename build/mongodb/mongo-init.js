db = db.getSiblingDB(process.env.GUESTBOOKY_DB_NAME);

db.createUser(
  {
    user: process.env.GUESTBOOKY_USER,
    pwd: process.env.GUESTBOOKY_PASSWORD,
    roles: [
      {
        role: "readWrite",
        db: process.env.GUESTBOOKY_DB_NAME
      }
    ]
  }
);

db.createCollection("GuestbookMessages");

db.GuestbookMessages.createIndex({ "Timestamp": 1 });