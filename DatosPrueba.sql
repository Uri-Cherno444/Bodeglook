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
('Sin TACC'), ('Vegetariano'), ('Vegano'), ('Sin lactosa'), ('Kosher');
GO

-- PRUEBA: restaurantes del Figma (pantallas 01, 03 y 06).
-- Solo Bodega Los Alamos tiene descripción, teléfono y horario en el Figma (pantalla 04).
-- La tabla no acepta NULL en dirección, descripción, teléfono, horario ni rango de precio,
-- así que donde el Figma no tiene el dato va '' o 0 (SIN DATO).
INSERT INTO Restaurantes (Nombre, DireccionCalle, DireccionNumero, DireccionCodigoPostal, Descripcion, Telefono,
                          HorarioApertura, HorarioCerrado, RangoPrecio, Foto, idBarrio, idCategoria)
VALUES
('Bodega Los Alamos', 'Cañuelas, Buenos Aires', 0, 0,
 'Bodega boutique con salón de degustación y patio al aire libre. Especialidad en vinos de la región y maridajes con tabla de quesos.',
 1155551234, '1900-01-01 12:00', '1900-01-01 23:00', 0, '/img/bodega-los-alamos.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Cañuelas, Bs. As.'), (SELECT ID FROM Categorias WHERE Nombre = 'Vinoteca')),

('El Rincón Criollo', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/el-rincon-criollo.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Palermo, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Parrilla')),

('Vinos del Sur', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/vinos-del-sur.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'San Telmo, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Bar de vinos')),

('Trattoria Nonna', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/trattoria-nonna.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Belgrano, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Italiana')),

('Asador Don Pepe', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/asador-don-pepe.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Recoleta, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Parrilla')),

('La Bodega del Puerto', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/la-bodega-del-puerto.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Puerto Madero, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Vinoteca')),

('Sushi Bar Nami', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/sushi-bar-nami.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Núñez, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Japonesa')),

('Cantina Vecchia', '', 0, 0, '', 0, '1900-01-01', '1900-01-01', 0, '/img/cantina-vecchia.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Caballito, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Italiana'));
GO

-- PRUEBA: "Sin TACC" y "Opción vegetariana" de Bodega Los Alamos (pantalla 04) y "Sin TACC" de Vinos del Sur (tarjeta del hero)
INSERT INTO RestriccionesRestaurantes (idRestaurante, idRestriccion) VALUES
((SELECT ID FROM Restaurantes WHERE Nombre = 'Bodega Los Alamos'), (SELECT ID FROM Restricciones WHERE Nombre = 'Sin TACC')),
((SELECT ID FROM Restaurantes WHERE Nombre = 'Bodega Los Alamos'), (SELECT ID FROM Restricciones WHERE Nombre = 'Vegetariano')),
((SELECT ID FROM Restaurantes WHERE Nombre = 'Vinos del Sur'), (SELECT ID FROM Restricciones WHERE Nombre = 'Sin TACC'));
GO

-- PRUEBA: Bodega Los Alamos es Pro (chip "Recomendado" en 05 · Descubrir)
UPDATE Restaurantes SET EsPro = 1 WHERE Nombre = 'Bodega Los Alamos';
GO

-- PRUEBA: coordenadas para el mapa de 05 · Descubrir.
-- Son APROXIMADAS: el centro del barrio de cada uno (las direcciones de prueba están vacías).
UPDATE Restaurantes SET Latitud = -35.051700, Longitud = -58.761400 WHERE Nombre = 'Bodega Los Alamos';
UPDATE Restaurantes SET Latitud = -34.588900, Longitud = -58.430000 WHERE Nombre = 'El Rincón Criollo';
UPDATE Restaurantes SET Latitud = -34.621200, Longitud = -58.373100 WHERE Nombre = 'Vinos del Sur';
UPDATE Restaurantes SET Latitud = -34.562700, Longitud = -58.456500 WHERE Nombre = 'Trattoria Nonna';
UPDATE Restaurantes SET Latitud = -34.587500, Longitud = -58.397400 WHERE Nombre = 'Asador Don Pepe';
UPDATE Restaurantes SET Latitud = -34.611800, Longitud = -58.362700 WHERE Nombre = 'La Bodega del Puerto';
UPDATE Restaurantes SET Latitud = -34.544600, Longitud = -58.463200 WHERE Nombre = 'Sushi Bar Nami';
UPDATE Restaurantes SET Latitud = -34.618900, Longitud = -58.442100 WHERE Nombre = 'Cantina Vecchia';
GO

-- =====================================================================
-- PRUEBA: usuarios, grupos, planes, votos, mensajes y favoritos
-- Todos los usuarios de prueba entran con la contraseña "Prueba123".
-- (Comparten el mismo hash porque son de prueba; los usuarios reales
--  se registran desde la página y cada uno tiene su propia sal.)
-- idBarrio queda en NULL: el registro no lo pide.
-- =====================================================================
INSERT INTO Usuarios (NombreCompleto, NombreUsuario, Email, Contraseña, FechaRegistro) VALUES
('Camila Fernández', 'camilaf', 'camilaf@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Julián Pérez', 'julianp', 'julianp@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Martina G.', 'martinag', 'martinag@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Nico Ramos', 'nicor', 'nicor@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Cami', 'cami', 'cami@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Sofi', 'sofi', 'sofi@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Fran', 'fran', 'fran@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE()),
('Tomás', 'tomas', 'tomas@prueba.com', '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+', GETDATE());
GO

-- PRUEBA: favoritos de Camila (los de "Tus favoritos" en 03 · Inicio)
-- (uno por uno para que queden en el mismo orden que en el Figma)
INSERT INTO Favoritos (idUsuario, idRestaurante) VALUES
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT ID FROM Restaurantes WHERE Nombre = 'La Bodega del Puerto')),
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Asador Don Pepe')),
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Sushi Bar Nami')),
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Cantina Vecchia'));
GO

-- PRUEBA: búsquedas recientes de Camila (las de 05 · Descubrir; la más nueva primero)
INSERT INTO Busquedas (idUsuario, Texto, Fecha) VALUES
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), 'Parrillas en Palermo', DATEADD(day, -3, GETDATE())),
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), 'Sushi Bar Nami', DATEADD(day, -2, GETDATE())),
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), 'Bodega Los Alamos', DATEADD(day, -1, GETDATE()));
GO

-- PRUEBA: las 3 reseñas de Bodega Los Alamos del Figma (pantallas 01 y 04)
INSERT INTO Reseñas (Puntaje, Comentario, Fecha, idUsuario, idRestaurante) VALUES
(5, 'Fuimos en grupo y se adaptaron a todos. Volvemos seguro.', DATEADD(day, -21, GETDATE()),
 (SELECT ID FROM Usuarios WHERE NombreUsuario = 'nicor'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Bodega Los Alamos')),
(4, 'Muy buena opción sin TACC, nos cuidaron mucho. Recomiendo ir temprano.', DATEADD(day, -7, GETDATE()),
 (SELECT ID FROM Usuarios WHERE NombreUsuario = 'martinag'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Bodega Los Alamos')),
(5, 'El patio es hermoso y la tabla de quesos con el malbec fue un golazo. Atención de 10.', DATEADD(day, -2, GETDATE()),
 (SELECT ID FROM Usuarios WHERE NombreUsuario = 'julianp'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Bodega Los Alamos'));
GO

-- PRUEBA: grupos del Figma y sus integrantes
INSERT INTO Grupos (Nombre, Foto, FechaCreacion) VALUES
('Grupo Colegio', NULL, GETDATE()), ('Grupo Club', NULL, GETDATE()), ('Grupo Random', NULL, GETDATE());
GO

INSERT INTO UsuariosGrupos (idUsuario, idGrupo)
SELECT u.ID, g.ID FROM Usuarios u, Grupos g
WHERE (g.Nombre = 'Grupo Colegio' AND u.NombreUsuario IN ('camilaf', 'cami', 'sofi', 'julianp'))
   OR (g.Nombre = 'Grupo Club' AND u.NombreUsuario IN ('camilaf', 'fran', 'tomas', 'martinag'))
   OR (g.Nombre = 'Grupo Random' AND u.NombreUsuario IN ('camilaf', 'nicor', 'martinag', 'julianp'));
GO

-- PRUEBA: planes (encuestas dentro de cada grupo). Estado: 0 = En votación, 1 = Confirmado.
-- FechaSalida lleva el día y la hora juntos.
DECLARE @hoy datetime = CAST(CAST(GETDATE() AS date) AS datetime);
INSERT INTO Planes (Titulo, FechaCierreVotacion, FechaSalida, Estado, idGrupo, idRestauranteElegido) VALUES
('Cena de cumpleaños', DATEADD(day, 3, @hoy), DATEADD(minute, 21*60, DATEADD(day, 5, @hoy)), 0,
 (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Colegio'), NULL),
('Salida de vinos', DATEADD(day, -1, @hoy), DATEADD(minute, 20*60, DATEADD(day, 3, @hoy)), 1,
 (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Club'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Vinos del Sur')),
('Cena con amigos', DATEADD(day, 5, @hoy), DATEADD(minute, 21*60+30, DATEADD(day, 7, @hoy)), 0,
 (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Random'), NULL);
GO

-- PRUEBA: lugares propuestos en cada plan
INSERT INTO PlanesRestaurantes (idPlan, idRestaurante)
SELECT p.ID, r.ID FROM Planes p, Restaurantes r
WHERE (p.Titulo = 'Cena de cumpleaños' AND r.Nombre IN ('Bodega Los Alamos', 'Vinos del Sur', 'Asador Don Pepe'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre IN ('Vinos del Sur', 'La Bodega del Puerto', 'El Rincón Criollo'))
   OR (p.Titulo = 'Cena con amigos' AND r.Nombre IN ('Trattoria Nonna', 'Cantina Vecchia', 'Sushi Bar Nami'));
GO

-- PRUEBA: votos
INSERT INTO Votos (idUsuario, idPlan, idRestaurante)
SELECT u.ID, p.ID, r.ID FROM Usuarios u, Planes p, Restaurantes r
WHERE (p.Titulo = 'Cena de cumpleaños' AND r.Nombre = 'Bodega Los Alamos' AND u.NombreUsuario IN ('sofi', 'camilaf'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre = 'Vinos del Sur' AND u.NombreUsuario IN ('camilaf', 'fran', 'tomas'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre = 'La Bodega del Puerto' AND u.NombreUsuario = 'martinag')
   OR (p.Titulo = 'Cena con amigos' AND r.Nombre = 'Trattoria Nonna' AND u.NombreUsuario = 'nicor');
GO

-- PRUEBA: mensajes de los chats
DECLARE @hoy datetime = CAST(CAST(GETDATE() AS date) AS datetime);
DECLARE @colegio int = (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Colegio');
DECLARE @club int = (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Club');
DECLARE @random int = (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Random');

INSERT INTO Mensajes (Contenido, FechaHora, idGrupo, idUsuario) VALUES
('¿Arrancamos el plan para el cumple de Sofi? Les mando opciones.', DATEADD(minute, 18*60+30, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'cami')),
('¡Dale! Yo voté por la bodega, tiene opciones sin TACC.', DATEADD(minute, 18*60+36, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'sofi')),
('Yo también voté Bodega Los Alamos. ¡Faltan dos!', DATEADD(minute, 18*60+42, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf')),
('¡Confirmado! Nos vemos el sábado.', DATEADD(minute, 17*60+15, @hoy), @club, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'fran')),
('Me encanta la trattoria, ¡voten!', DATEADD(minute, 20*60+10, DATEADD(day, -1, @hoy)), @random, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'martinag'));

-- PRUEBA: hasta dónde leyó Camila cada chat (Colegio: 1 sin leer, Club: 1, Random: 0)
UPDATE UsuariosGrupos SET UltimaLectura = DATEADD(minute, 18*60+40, @hoy)
WHERE idGrupo = @colegio AND idUsuario = (SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf');
UPDATE UsuariosGrupos SET UltimaLectura = DATEADD(minute, 17*60, @hoy)
WHERE idGrupo = @club AND idUsuario = (SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf');
UPDATE UsuariosGrupos SET UltimaLectura = GETDATE()
WHERE idGrupo = @random AND idUsuario = (SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf');
GO

-- ---------------------------------------------------------------------
-- Para BORRAR los datos de prueba, descomentar y correr:
-- ---------------------------------------------------------------------
-- DELETE FROM Mensajes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM Votos WHERE idPlan IN (SELECT ID FROM Planes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random')));
-- DELETE FROM PlanesRestaurantes WHERE idPlan IN (SELECT ID FROM Planes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random')));
-- DELETE FROM Planes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM UsuariosGrupos WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random');
-- DELETE FROM Reseñas WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Favoritos WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Busquedas WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Usuarios WHERE Email LIKE '%@prueba.com';
-- DELETE FROM RestriccionesRestaurantes WHERE idRestaurante IN (SELECT ID FROM Restaurantes WHERE Nombre IN ('Bodega Los Alamos','El Rincón Criollo','Vinos del Sur','Trattoria Nonna','Asador Don Pepe','La Bodega del Puerto','Sushi Bar Nami','Cantina Vecchia'));
-- DELETE FROM Restaurantes WHERE Nombre IN ('Bodega Los Alamos','El Rincón Criollo','Vinos del Sur','Trattoria Nonna','Asador Don Pepe','La Bodega del Puerto','Sushi Bar Nami','Cantina Vecchia');
-- DELETE FROM Restricciones WHERE Nombre IN ('Sin TACC','Vegetariano','Vegano','Sin lactosa','Kosher');
-- DELETE FROM Barrios WHERE Nombre IN ('Cañuelas, Bs. As.','Palermo, CABA','San Telmo, CABA','Belgrano, CABA','Recoleta, CABA','Puerto Madero, CABA','Núñez, CABA','Caballito, CABA');
-- DELETE FROM Categorias WHERE Nombre IN ('Vinoteca','Parrilla','Bar de vinos','Italiana','Japonesa');
