db = db.getSiblingDB('Guestbooky');

db.createUser(
  {
    user: "guestbookyuser",
    pwd: "guestbookypassword",
    roles: [
      {
        role: "readWrite",
        db: "Guestbooky"
      }
    ]
  }
);

db.createCollection("GuestbookMessages");

db.GuestbookMessages.createIndex({ "Timestamp": 1 });