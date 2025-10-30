-- Umetanje podataka na srpskom jeziku u tabelu 'genres'.
-- NAPOMENA: Za žanrove koji nisu podžanrovi (koreni žanrovi), parentGenreId je postavljen na NULL.
-- UUID-ovi su generisani za potrebe demonstracije.

-- =========================================================
-- KORENI ŽANROVI (ParentID = NULL)
-- =========================================================

-- 1. Pop
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('8f1c4e7d-9a6b-4c2f-8d0e-1a3b5c7d9f1e', 'Pop', 'Popularna muzika, obično komercijalno orijentisana i namenjena širokoj publici.', NULL);

-- 2. Rock
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('c3d8a7b6-5e4f-3d2c-1b0a-9f8e7d6c5b4a', 'Rok', 'Žanr popularne muzike koji se razvio 1950-ih i 1960-ih, sa naglaskom na električne gitare i snažan ritam.', NULL);

-- 3. Klasična muzika
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('7b34b9e2-3a78-4d5c-9c0f-2e1d0f5a4b3c', 'Klasična muzika', 'Muzika proizvedena u evropskoj tradiciji umetničke muzike, od srednjeg veka do danas.', NULL);

-- 4. Jazz
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('3a4c5b6d-7e8f-9a0b-1c2d-3e4f5a6b7c8d', 'Džez', 'Muzički stil nastao krajem 19. i početkom 20. veka u afroameričkim zajednicama u SAD.', NULL);

-- 5. Hip Hop
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('2e5f8a1b-3c4d-5e6f-7a8b-9c0d1e2f3a4b', 'Hip Hop', 'Kulturni pokret i muzički žanr karakterističan po ritmičnom i rimovanom govoru (repu) uz ritam.', NULL);

-- 6. Elektronska muzika
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('7e9f1a3b-5c7d-9e1f-3a5b-7c9d1e3f5a7b', 'Elektronska muzika', 'Muzika stvorena, modifikovana ili izvođena korišćenjem elektronskih muzičkih instrumenata.', NULL);

-- 7. Narodna muzika
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d', 'Narodna muzika', 'Tradicionalna muzika koja se prenosi usmeno, često vezana za specifičnu kulturu ili region.', NULL);

-- =========================================================
-- PODŽANROVI (ParentID je ID roditelja)
-- =========================================================

-- 8. Opera (dete Klasične muzike)
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('1f9d6c7a-8b0e-4f1d-9c3a-5b4d6e7f8a9b', 'Opera', 'Dramsko delo u kome se koristi muzika, pevanje i scenski nastup; deo Klasične muzike.', '7b34b9e2-3a78-4d5c-9c0f-2e1d0f5a4b3c');

-- 9. Simfonijska muzika (dete Klasične muzike)
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('4d6e9f2a-1b3c-5d7e-8f0a-2b4c6d8e0f1a', 'Simfonijska muzika', 'Veliko muzičko delo za orkestar, tipično u četiri stava; deo Klasične muzike.', '7b34b9e2-3a78-4d5c-9c0f-2e1d0f5a4b3c');

-- 10. Pop Rok (dete Roka)
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('5c7d9f1e-8a6b-4c2f-0d3e-1b5c7d9f1e8a', 'Pop Rok', 'Muzika koja kombinuje optimistične elemente Pop muzike sa gitarama orijentisanim na Rok.', 'c3d8a7b6-5e4f-3d2c-1b0a-9f8e7d6c5b4a');

-- 11. Alternativni rok (dete Roka)
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('6a8b0c2d-4e6f-8a0b-2c4d-6e8f0a2b4c6d', 'Alternativni rok', 'Rok žanr koji se pojavio 1980-ih, definisan odstupanjem od mejnstrim Rok stila.', 'c3d8a7b6-5e4f-3d2c-1b0a-9f8e7d6c5b4a');

-- 12. House (dete Elektronske muzike)
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('8c0d2e4f-6a8b-0c2d-4e6f-8a0b2c4d6e8f', 'Haus', 'Stil Elektronske plesne muzike stvoren u Čikagu ranih 1980-ih, karakterističan po brzim ritmovima.', '7e9f1a3b-5c7d-9e1f-3a5b-7c9d1e3f5a7b');

-- 13. Rep (dete Hip Hopa)
INSERT INTO genres (genreId, genreName, genreDescription, parentGenreId)
VALUES ('9d1e3f5a-7b9c-1d3e-5f7a-9b1c3d5e7f9a', 'Rep', 'Stilizovani ritmički govor koji je centralni element Hip Hop muzike.', '2e5f8a1b-3c4d-5e6f-7a8b-9c0d1e2f3a4b');
