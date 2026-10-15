-- =====================================================================
-- DATOS DE PRUEBA de Bodeglook
-- Todo lo que inserta este script es DE PRUEBA y sale del Figma.
-- No hay columna para marcar filas de prueba, así que se identifican
-- por estos nombres (ver el bloque para borrarlos al final).
-- Correr con: sqlcmd -S localhost -E -f 65001 -i DatosPrueba.sql
-- =====================================================================

USE [Bodeglook]
GO

-- PRUEBA: categorías que aparecen en el Figma
INSERT INTO Categorias (Nombre) VALUES
('Vinoteca'), ('Parrilla'), ('Bar de vinos'), ('Italiana'), ('Japonesa');
GO

-- PRUEBA: ubicaciones tal como aparecen en las tarjetas del Figma
INSERT INTO Barrios (Nombre) VALUES
('Cañuelas, Bs. As.'), ('Palermo, CABA'), ('San Telmo, CABA'), ('Belgrano, CABA'), ('Recoleta, CABA'),
('Puerto Madero, CABA'), ('Núñez, CABA'), ('Caballito, CABA');
GO

-- PRUEBA: filtros dietarios del Figma
INSERT INTO Restricciones (Nombre) VALUES
('Sin TACC'), ('Vegetariano'), ('Vegano'), ('Sin lactosa');
GO

-- PRUEBA: restaurantes del Figma (pantallas 01, 03 y 06).
-- Solo Bodega Los Alamos tiene descripción, teléfono y horario en el Figma (pantalla 04);
-- en los demás esos datos quedan en NULL.
INSERT INTO Restaurantes (Nombre, Direccion, Descripcion, Telefono, Horario, RangoPrecio, Foto, IdBarrio, IdCategoria)
VALUES
('Bodega Los Alamos', 'Cañuelas, Buenos Aires',
 'Bodega boutique con salón de degustación y patio al aire libre. Especialidad en vinos de la región y maridajes con tabla de quesos.',
 '+54 11 5555-1234', '12:00 - 23:00', NULL, '/img/bodega-los-alamos.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Cañuelas, Bs. As.'), (SELECT Id FROM Categorias WHERE Nombre = 'Vinoteca')),

('El Rincón Criollo', NULL, NULL, NULL, NULL, NULL, '/img/el-rincon-criollo.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Palermo, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Parrilla')),

('Vinos del Sur', NULL, NULL, NULL, NULL, NULL, '/img/vinos-del-sur.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'San Telmo, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Bar de vinos')),

('Trattoria Nonna', NULL, NULL, NULL, NULL, NULL, '/img/trattoria-nonna.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Belgrano, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Italiana')),

('Asador Don Pepe', NULL, NULL, NULL, NULL, NULL, '/img/asador-don-pepe.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Recoleta, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Parrilla')),

('La Bodega del Puerto', NULL, NULL, NULL, NULL, NULL, '/img/la-bodega-del-puerto.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Puerto Madero, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Vinoteca')),

('Sushi Bar Nami', NULL, NULL, NULL, NULL, NULL, '/img/sushi-bar-nami.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Núñez, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Japonesa')),

('Cantina Vecchia', NULL, NULL, NULL, NULL, NULL, '/img/cantina-vecchia.jpg',
 (SELECT Id FROM Barrios WHERE Nombre = 'Caballito, CABA'), (SELECT Id FROM Categorias WHERE Nombre = 'Italiana'));
GO

-- PRUEBA: "Sin TACC" de Bodega Los Alamos (pantalla 04) y Vinos del Sur (tarjeta del hero)
INSERT INTO RestaurantesRestricciones (IdRestaurante, IdRestriccion) VALUES
((SELECT Id FROM Restaurantes WHERE Nombre = 'Bodega Los Alamos'), (SELECT Id FROM Restricciones WHERE Nombre = 'Sin TACC')),
((SELECT Id FROM Restaurantes WHERE Nombre = 'Vinos del Sur'), (SELECT Id FROM Restricciones WHERE Nombre = 'Sin TACC'));
GO

-- =====================================================================
-- PRUEBA: usuarios, grupos, amigos, planes, votos, mensajes y favoritos
-- Todos los usuarios de prueba entran con la contraseña "Prueba123".
-- (Comparten el mismo hash porque son de prueba; los usuarios reales
--  se registran desde la página y cada uno tiene su propia sal.)
-- =====================================================================
INSERT INTO Usuarios (Nombre, NombreUsuario, Email, Contraseña, FechaRegistro) VALUES
('Camila Fernández', 'camilaf', 'camilaf@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Julián Pérez', 'julianp', 'julianp@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Martina G.', 'martinag', 'martinag@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Nico Ramos', 'nicor', 'nicor@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Cami', 'cami', 'cami@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Sofi', 'sofi', 'sofi@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Fran', 'fran', 'fran@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE()),
('Tomás', 'tomas', 'tomas@prueba.com', 'vNxAhp6koX6/35mSvIvvqA==:t9sBVTlcRwKyzntJ3Go6WHTDNwQR7tNknfaE3Acd65s=', GETDATE());
GO

-- PRUEBA: amigos de Camila
INSERT INTO Amigos (IdUsuario, IdAmigo, Fecha)
SELECT (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), Id, GETDATE()
FROM Usuarios WHERE NombreUsuario IN ('julianp', 'martinag', 'nicor', 'cami', 'sofi');
GO

-- PRUEBA: favoritos de Camila (los de "Tus favoritos" en 03 · Inicio)
-- (uno por uno para que queden en el mismo orden que en el Figma)
INSERT INTO Favoritos (IdUsuario, IdRestaurante) VALUES
((SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT Id FROM Restaurantes WHERE Nombre = 'La Bodega del Puerto')),
((SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT Id FROM Restaurantes WHERE Nombre = 'Asador Don Pepe')),
((SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT Id FROM Restaurantes WHERE Nombre = 'Sushi Bar Nami')),
((SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT Id FROM Restaurantes WHERE Nombre = 'Cantina Vecchia'));
GO

-- PRUEBA: grupos del Figma y sus integrantes
INSERT INTO Grupos (Nombre, Foto, FechaCreacion) VALUES
('Grupo Colegio', NULL, GETDATE()), ('Grupo Club', NULL, GETDATE()), ('Grupo Random', NULL, GETDATE());
GO

INSERT INTO UsuariosGrupos (IdUsuario, IdGrupo)
SELECT u.Id, g.Id FROM Usuarios u, Grupos g
WHERE (g.Nombre = 'Grupo Colegio' AND u.NombreUsuario IN ('camilaf', 'cami', 'sofi', 'julianp'))
   OR (g.Nombre = 'Grupo Club' AND u.NombreUsuario IN ('camilaf', 'fran', 'tomas', 'martinag'))
   OR (g.Nombre = 'Grupo Random' AND u.NombreUsuario IN ('camilaf', 'nicor', 'martinag', 'julianp'));
GO

-- PRUEBA: planes (encuestas dentro de cada grupo)
INSERT INTO Planes (Titulo, FechaSalida, HoraSalida, Estado, IdGrupo, IdRestauranteElegido, IdUsuarioCreador) VALUES
('Cena de cumpleaños', DATEADD(day, 5, GETDATE()), '21:00', 'En votación',
 (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Colegio'), NULL, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'cami')),
('Salida de vinos', DATEADD(day, 3, GETDATE()), '20:00', 'Confirmado',
 (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Club'), (SELECT Id FROM Restaurantes WHERE Nombre = 'Vinos del Sur'), (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf')),
('Cena con amigos', DATEADD(day, 7, GETDATE()), '21:30', 'En votación',
 (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Random'), NULL, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'nicor')),
('Almuerzo del domingo', DATEADD(day, 4, GETDATE()), '13:00', 'Cancelado',
 (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Club'), NULL, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'fran'));
GO

-- PRUEBA: lugares propuestos en cada plan
INSERT INTO PlanesRestaurantes (IdPlan, IdRestaurante)
SELECT p.Id, r.Id FROM Planes p, Restaurantes r
WHERE (p.Titulo = 'Cena de cumpleaños' AND r.Nombre IN ('Bodega Los Alamos', 'Vinos del Sur', 'Asador Don Pepe'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre IN ('Vinos del Sur', 'La Bodega del Puerto'))
   OR (p.Titulo = 'Cena con amigos' AND r.Nombre IN ('Trattoria Nonna', 'Cantina Vecchia'))
   OR (p.Titulo = 'Almuerzo del domingo' AND r.Nombre IN ('El Rincón Criollo'));
GO

-- PRUEBA: votos
INSERT INTO Votos (IdUsuario, IdPlan, IdRestaurante)
SELECT u.Id, p.Id, r.Id FROM Usuarios u, Planes p, Restaurantes r
WHERE (p.Titulo = 'Cena de cumpleaños' AND r.Nombre = 'Bodega Los Alamos' AND u.NombreUsuario IN ('sofi', 'camilaf'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre = 'Vinos del Sur' AND u.NombreUsuario IN ('camilaf', 'fran', 'tomas'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre = 'La Bodega del Puerto' AND u.NombreUsuario = 'martinag')
   OR (p.Titulo = 'Cena con amigos' AND r.Nombre = 'Trattoria Nonna' AND u.NombreUsuario = 'nicor');
GO

-- PRUEBA: mensajes (los que tienen IdPlan son la encuesta del plan dentro del chat)
DECLARE @hoy datetime = CAST(CAST(GETDATE() AS date) AS datetime);
DECLARE @colegio int = (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Colegio');
DECLARE @club int = (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Club');
DECLARE @random int = (SELECT Id FROM Grupos WHERE Nombre = 'Grupo Random');

INSERT INTO Mensajes (Contenido, FechaHora, IdGrupo, IdUsuario, IdPlan) VALUES
('¿Arrancamos el plan para el cumple de Sofi? Les mando opciones.', DATEADD(minute, 18*60+30, @hoy), @colegio, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'cami'), NULL),
(NULL, DATEADD(minute, 18*60+31, @hoy), @colegio, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'cami'), (SELECT Id FROM Planes WHERE Titulo = 'Cena de cumpleaños')),
('¡Dale! Yo voté por la bodega, tiene opciones sin TACC.', DATEADD(minute, 18*60+36, @hoy), @colegio, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'sofi'), NULL),
('Yo también voté Bodega Los Alamos. ¡Faltan dos!', DATEADD(minute, 18*60+42, @hoy), @colegio, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), NULL),
(NULL, DATEADD(minute, 19*60, DATEADD(day, -2, @hoy)), @club, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT Id FROM Planes WHERE Titulo = 'Salida de vinos')),
('¡Confirmado! Nos vemos el sábado.', DATEADD(minute, 17*60+15, @hoy), @club, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'fran'), NULL),
(NULL, DATEADD(minute, 20*60, DATEADD(day, -1, @hoy)), @random, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'nicor'), (SELECT Id FROM Planes WHERE Titulo = 'Cena con amigos')),
('Me encanta la trattoria, ¡voten!', DATEADD(minute, 20*60+10, DATEADD(day, -1, @hoy)), @random, (SELECT Id FROM Usuarios WHERE NombreUsuario = 'martinag'), NULL);

-- PRUEBA: hasta dónde leyó Camila cada chat (Colegio: 2 sin leer, Club: 1, Random: 0)
UPDATE UsuariosGrupos SET UltimaLectura = DATEADD(second, 18*3600+30*60+30, @hoy)
WHERE IdGrupo = @colegio AND IdUsuario = (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf');
UPDATE UsuariosGrupos SET UltimaLectura = DATEADD(minute, 17*60, @hoy)
WHERE IdGrupo = @club AND IdUsuario = (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf');
UPDATE UsuariosGrupos SET UltimaLectura = GETDATE()
WHERE IdGrupo = @random AND IdUsuario = (SELECT Id FROM Usuarios WHERE NombreUsuario = 'camilaf');
GO

-- ---------------------------------------------------------------------
-- Para BORRAR los datos de prueba, descomentar y correr:
-- ---------------------------------------------------------------------
-- DELETE FROM Mensajes WHERE IdGrupo IN (SELECT Id FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM Votos WHERE IdPlan IN (SELECT Id FROM Planes WHERE IdGrupo IN (SELECT Id FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random')));
-- DELETE FROM PlanesRestaurantes WHERE IdPlan IN (SELECT Id FROM Planes WHERE IdGrupo IN (SELECT Id FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random')));
-- DELETE FROM Planes WHERE IdGrupo IN (SELECT Id FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM UsuariosGrupos WHERE IdGrupo IN (SELECT Id FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random');
-- DELETE FROM Favoritos WHERE IdUsuario IN (SELECT Id FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Amigos WHERE IdUsuario IN (SELECT Id FROM Usuarios WHERE Email LIKE '%@prueba.com') OR IdAmigo IN (SELECT Id FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Usuarios WHERE Email LIKE '%@prueba.com';
-- DELETE FROM RestaurantesRestricciones WHERE IdRestaurante IN (SELECT Id FROM Restaurantes WHERE Nombre IN ('Bodega Los Alamos','El Rincón Criollo','Vinos del Sur','Trattoria Nonna','Asador Don Pepe','La Bodega del Puerto','Sushi Bar Nami','Cantina Vecchia'));
-- DELETE FROM Restaurantes WHERE Nombre IN ('Bodega Los Alamos','El Rincón Criollo','Vinos del Sur','Trattoria Nonna','Asador Don Pepe','La Bodega del Puerto','Sushi Bar Nami','Cantina Vecchia');
-- DELETE FROM Restricciones WHERE Nombre IN ('Sin TACC','Vegetariano','Vegano','Sin lactosa');
-- DELETE FROM Barrios WHERE Nombre IN ('Cañuelas, Bs. As.','Palermo, CABA','San Telmo, CABA','Belgrano, CABA','Recoleta, CABA','Puerto Madero, CABA','Núñez, CABA','Caballito, CABA');
-- DELETE FROM Categorias WHERE Nombre IN ('Vinoteca','Parrilla','Bar de vinos','Italiana','Japonesa');
