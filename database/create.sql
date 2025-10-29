-- PostgreSQL Translation with Pluralized, Lowercase Table Names

-- Table: people
CREATE TABLE people 
( 
  personId      VARCHAR(50)   NOT NULL PRIMARY KEY, 
  personName    TEXT          NOT NULL, 
  personSurname TEXT          NOT NULL 
);

---

-- Table: authors
CREATE TABLE authors 
( 
  authorId   VARCHAR(50)   NOT NULL PRIMARY KEY, 
  authorName TEXT, 
  biography  TEXT, 
  personId   VARCHAR(50)
);

-- Unique index for personId in authors
CREATE UNIQUE INDEX authors_personId_idx ON authors (personId);

-- Foreign key constraint for authors
ALTER TABLE authors 
  ADD CONSTRAINT authors_people_fk 
  FOREIGN KEY (personId) 
  REFERENCES people (personId);

---

-- Table: genres
CREATE TABLE genres 
( 
  genreId          VARCHAR(50)   NOT NULL PRIMARY KEY, 
  genreName        TEXT          NOT NULL, 
  genreDescription TEXT, 
  parentGenreId    VARCHAR(50) 
);

-- Foreign key constraint for self-referencing parentGenreId
ALTER TABLE genres 
  ADD CONSTRAINT genres_parent_fk 
  FOREIGN KEY (parentGenreId) 
  REFERENCES genres (genreId) 
  ON DELETE CASCADE;

---

-- Table: users (replaces "USER")
CREATE TABLE users 
( 
  userId           VARCHAR(50)   NOT NULL PRIMARY KEY, 
  email            TEXT          NOT NULL, 
  password         TEXT          NOT NULL, 
  role             TEXT          NOT NULL DEFAULT 'NORMAL', 
  personId         VARCHAR(50)   NOT NULL, 
  userStatus       TEXT          NOT NULL DEFAULT 'WAITING_VERIFICATION', 
  contactVisible   BOOLEAN,
  reviewsVisible   BOOLEAN,
  verificationCode TEXT 
);

-- Check constraints for users
ALTER TABLE users 
  ADD CONSTRAINT users_role_check 
  CHECK (role IN ('EDITOR', 'NORMAL_USER', 'ADMIN'));

ALTER TABLE users 
  ADD CONSTRAINT users_status_check 
  CHECK (userStatus IN ('ACTIVE', 'BANNED', 'DEACTIVATED', 'WAITING_VERIFICATION'));

ALTER TABLE users 
  ADD CONSTRAINT users_exdep 
  CHECK ( (role = 'EDITOR' AND contactVisible IS NULL AND reviewsVisible IS NULL AND verificationCode IS NULL)
          OR (role = 'NORMAL_USER')
          OR (role = 'ADMIN' AND contactVisible IS NULL AND reviewsVisible IS NULL AND verificationCode IS NULL));

-- Unique index for personId in users
CREATE UNIQUE INDEX users_personId_idx ON users (personId);

-- Foreign key constraint for users
ALTER TABLE users 
  ADD CONSTRAINT users_people_fk 
  FOREIGN KEY (personId) 
  REFERENCES people (personId);

---

-- Table: works
CREATE TABLE works 
( 
  workId          VARCHAR(50)   NOT NULL PRIMARY KEY, 
  workName        TEXT          NOT NULL, 
  publishDate     DATE          NOT NULL, 
  workType        TEXT          NOT NULL DEFAULT 'SONG', 
  workDescription TEXT, 
  src             TEXT, 
  albumId         VARCHAR(50), 
  genreId         VARCHAR(50)   NOT NULL 
);

-- Check constraint for works
ALTER TABLE works 
  ADD CONSTRAINT works_type_check 
  CHECK (workType IN ('ALBUM', 'SONG'));

-- Foreign key constraints for works
ALTER TABLE works 
  ADD CONSTRAINT works_genres_fk 
  FOREIGN KEY (genreId) 
  REFERENCES genres (genreId);

ALTER TABLE works 
  ADD CONSTRAINT works_album_fk 
  FOREIGN KEY (albumId) 
  REFERENCES works (workId);

---

-- Table: belongs
CREATE TABLE belongs 
( 
  authorId VARCHAR(50)   NOT NULL, 
  genreId  VARCHAR(50)   NOT NULL, 
  CONSTRAINT belongs_pk PRIMARY KEY (authorId, genreId),
  CONSTRAINT belongs_authors_fk FOREIGN KEY (authorId) REFERENCES authors (authorId),
  CONSTRAINT belongs_genres_fk FOREIGN KEY (genreId) REFERENCES genres (genreId)
);

---

-- Table: contributors
CREATE TABLE contributors 
( 
  contributionType TEXT          NOT NULL, 
  personId         VARCHAR(50)   NOT NULL, 
  workId           VARCHAR(50)   NOT NULL,
  CONSTRAINT contributors_pk PRIMARY KEY (personId, workId)
);

-- Check constraint for contributors
ALTER TABLE contributors 
  ADD CONSTRAINT contributors_type_check
  CHECK (contributionType IN ('ARRANGER', 'PRODUCER', 'SOUND_ENGINEER', 'WRITER'));

-- Foreign key constraints for contributors
ALTER TABLE contributors 
  ADD CONSTRAINT contributors_people_fk FOREIGN KEY (personId) REFERENCES people (personId);

ALTER TABLE contributors 
  ADD CONSTRAINT contributors_works_fk FOREIGN KEY (workId) REFERENCES works (workId);

---

-- Table: members
CREATE TABLE members 
( 
  joinDate  DATE          NOT NULL, 
  leaveDate DATE, 
  groupId   VARCHAR(50)   NOT NULL, 
  memberId  VARCHAR(50)   NOT NULL, 
  CONSTRAINT members_pk PRIMARY KEY (groupId, memberId),
  CONSTRAINT members_group_fk FOREIGN KEY (groupId) REFERENCES authors (authorId),
  CONSTRAINT members_member_fk FOREIGN KEY (memberId) REFERENCES authors (authorId)
);

---

-- Table: performs
CREATE TABLE performs 
( 
  authorId VARCHAR(50)   NOT NULL, 
  workId   VARCHAR(50)   NOT NULL,
  CONSTRAINT performs_pk PRIMARY KEY (authorId, workId),
  CONSTRAINT performs_authors_fk FOREIGN KEY (authorId) REFERENCES authors (authorId),
  CONSTRAINT performs_works_fk FOREIGN KEY (workId) REFERENCES works (workId)
);

---

-- Table: qualifications
CREATE TABLE qualifications 
( 
  userId  VARCHAR(50)   NOT NULL, 
  genreId VARCHAR(50)   NOT NULL,
  CONSTRAINT qualifications_pk PRIMARY KEY (userId, genreId),
  CONSTRAINT qualifications_users_fk FOREIGN KEY (userId) REFERENCES users (userId),
  CONSTRAINT qualifications_genres_fk FOREIGN KEY (genreId) REFERENCES genres (genreId)
);

---

-- Table: ratings
CREATE TABLE ratings 
( 
  ratingId   VARCHAR(50)   NOT NULL PRIMARY KEY, 
  value      NUMERIC       NOT NULL,
  ratingDate DATE          NOT NULL, 
  userId     VARCHAR(50)   NOT NULL, 
  workId     VARCHAR(50), 
  authorId   VARCHAR(50)
);

-- Check constraint Arc_3 for ratings (ensures rating is for a work OR an author, OR neither)
ALTER TABLE ratings 
  ADD CONSTRAINT ratings_arc_3 
  CHECK ( 
    ( (workId IS NOT NULL) AND (authorId IS NULL) ) OR 
    ( (authorId IS NOT NULL) AND (workId IS NULL) )
  );

-- Foreign key constraints for ratings
ALTER TABLE ratings 
  ADD CONSTRAINT ratings_users_fk FOREIGN KEY (userId) REFERENCES users (userId);

ALTER TABLE ratings 
  ADD CONSTRAINT ratings_works_fk FOREIGN KEY (workId) REFERENCES works (workId);

ALTER TABLE ratings 
  ADD CONSTRAINT ratings_authors_fk FOREIGN KEY (authorId) REFERENCES authors (authorId);

---

-- Table: reviews
CREATE TABLE reviews 
( 
  reviewId   VARCHAR(50)   NOT NULL PRIMARY KEY, 
  content    TEXT          NOT NULL, 
  reviewDate DATE          NOT NULL, 
  isEditable BOOLEAN       NOT NULL,
  userId    VARCHAR(50)   NOT NULL, 
  editorId   VARCHAR(50), 
  workId     VARCHAR(50), 
  authorId   VARCHAR(50) 
);

-- Check constraint Arc_2 for reviews (same logic as ratings)
ALTER TABLE reviews 
  ADD CONSTRAINT reviews_arc_2 
  CHECK ( 
    ( (workId IS NOT NULL) AND (authorId IS NULL) ) OR 
    ( (authorId IS NOT NULL) AND (workId IS NULL) )
  );

-- Foreign key constraints for reviews
ALTER TABLE reviews 
  ADD CONSTRAINT reviews_users_fk FOREIGN KEY (userId) REFERENCES users (userId);

ALTER TABLE reviews 
  ADD CONSTRAINT reviews_users_fk_v2 FOREIGN KEY (userId2) REFERENCES users (userId);

ALTER TABLE reviews 
  ADD CONSTRAINT reviews_works_fk FOREIGN KEY (workId) REFERENCES works (workId);

ALTER TABLE reviews 
  ADD CONSTRAINT reviews_authors_fk FOREIGN KEY (authorId) REFERENCES authors (authorId);

---

-- Table: tasks
CREATE TABLE tasks 
( 
  taskId         VARCHAR(50)   NOT NULL PRIMARY KEY, 
  assignmentDate DATE          NOT NULL, 
  done           BOOLEAN       NOT NULL,
  userId         VARCHAR(50)   NOT NULL, 
  workId         VARCHAR(50), 
  authorId       VARCHAR(50) 
);

-- Check constraint Arc_1 for tasks (same logic as ratings/reviews)
ALTER TABLE tasks 
  ADD CONSTRAINT tasks_arc_1 
  CHECK ( 
    ( (authorId IS NOT NULL) AND (workId IS NULL) ) OR 
    ( (workId IS NOT NULL) AND (authorId IS NULL) )
  );

-- Foreign key constraints for tasks
ALTER TABLE tasks 
  ADD CONSTRAINT tasks_users_fk FOREIGN KEY (userId) REFERENCES users (userId);

ALTER TABLE tasks 
  ADD CONSTRAINT tasks_works_fk FOREIGN KEY (workId) REFERENCES works (workId);

ALTER TABLE tasks 
  ADD CONSTRAINT tasks_authors_fk FOREIGN KEY (authorId) REFERENCES authors (authorId);
