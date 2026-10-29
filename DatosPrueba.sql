-- =====================================================================
-- DATOS DE PRUEBA de Bodeglook
-- Todo lo que inserta este script es DE PRUEBA y sale del Figma.
-- No hay columna para marcar filas de prueba, así que se identifican
-- por estos nombres (ver el bloque para borrarlos al final).
-- Correr con: sqlcmd -S localhost -E -f 65001 -i DatosPrueba.sql
-- =====================================================================

USE [Bodeglook]
GO

-- PRUEBA: categorías que aparecen en el Figma (solo las que Bodeglook.sql no trae; Parrilla ya está)
INSERT INTO Categorias (Nombre)
SELECT c.Nombre FROM (VALUES ('Vinoteca'), ('Parrilla'), ('Bar de vinos'), ('Italiana'), ('Japonesa')) c(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM Categorias WHERE Nombre = c.Nombre);
GO

-- PRUEBA: ubicaciones tal como aparecen en las tarjetas del Figma
INSERT INTO Barrios (Nombre) VALUES
('Cañuelas, Bs. As.'), ('Palermo, CABA'), ('San Telmo, CABA'), ('Belgrano, CABA'), ('Recoleta, CABA'),
('Puerto Madero, CABA'), ('Núñez, CABA'), ('Caballito, CABA');
GO

-- PRUEBA: filtros dietarios del Figma (solo los que Bodeglook.sql no trae)
INSERT INTO Restricciones (Nombre)
SELECT r.Nombre FROM (VALUES ('Sin TACC'), ('Vegetariano'), ('Vegano'), ('Sin lactosa'), ('Kosher')) r(Nombre)
WHERE NOT EXISTS (SELECT 1 FROM Restricciones WHERE Nombre = r.Nombre);
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
 '1155551234', '1900-01-01 12:00', '1900-01-01 23:00', 0, '/img/bodega-los-alamos.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Cañuelas, Bs. As.'), (SELECT ID FROM Categorias WHERE Nombre = 'Vinoteca')),

('El Rincón Criollo', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/el-rincon-criollo.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Palermo, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Parrilla')),

('Vinos del Sur', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/vinos-del-sur.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'San Telmo, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Bar de vinos')),

('Trattoria Nonna', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/trattoria-nonna.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Belgrano, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Italiana')),

('Asador Don Pepe', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/asador-don-pepe.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Recoleta, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Parrilla')),

('La Bodega del Puerto', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/la-bodega-del-puerto.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Puerto Madero, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Vinoteca')),

('Sushi Bar Nami', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/sushi-bar-nami.jpg',
 (SELECT ID FROM Barrios WHERE Nombre = 'Núñez, CABA'), (SELECT ID FROM Categorias WHERE Nombre = 'Japonesa')),

('Cantina Vecchia', '', 0, 0, '', '', '1900-01-01', '1900-01-01', 0, '/img/cantina-vecchia.jpg',
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

-- PRUEBA: los usuarios que trae Bodeglook.sql tienen la contraseña en texto plano ('lucia2026').
-- El login compara contra el hash, así que se les pone el de "Prueba123" (los hash tienen ':', el texto plano no).
UPDATE Usuarios SET Contraseña = '0AXIfqPnNsQ5NO0l:73Ot3VhNhgOb+X1o9Oo4XEQHRuCSLdd+'
WHERE CHARINDEX(':', Contraseña) = 0;
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

-- PRUEBA: en Grupo Colegio hay una celíaca y una vegetariana (07c · "Aptos para todo el grupo")
INSERT INTO RestriccionesUsuarios (idUsuario, idRestricciones) VALUES
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'sofi'), (SELECT ID FROM Restricciones WHERE Nombre = 'Sin TACC')),
((SELECT ID FROM Usuarios WHERE NombreUsuario = 'cami'), (SELECT ID FROM Restricciones WHERE Nombre = 'Vegetariano'));
GO

-- PRUEBA: web de Bodega Los Alamos ("Reservar en la web del restaurante" en 07i). Es una dirección de ejemplo.
UPDATE Restaurantes SET SitioWeb = 'https://example.com/bodega-los-alamos' WHERE Nombre = 'Bodega Los Alamos';
GO

-- PRUEBA: planes (encuestas dentro de cada grupo). Estado: 0 = En votación, 1 = Confirmado, 2 = Cancelado.
-- Mientras se vota, FechaSalida es solo el día (a las 00:00); al confirmar pasa a ser el día y horario ganador.
-- FechaCierreVotacion es la misma que FechaSalida. idUsuarioCreador es quien creó el plan: es el único que lo confirma.
DECLARE @hoy datetime = CAST(CAST(GETDATE() AS date) AS datetime);
INSERT INTO Planes (Titulo, FechaCierreVotacion, FechaSalida, Estado, idGrupo, idRestauranteElegido, idUsuarioCreador) VALUES
('Cena de cumpleaños', DATEADD(day, 5, @hoy), DATEADD(day, 5, @hoy), 0,
 (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Colegio'), NULL, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'cami')),
('Salida de vinos', DATEADD(minute, 20*60, DATEADD(day, 3, @hoy)), DATEADD(minute, 20*60, DATEADD(day, 3, @hoy)), 1,
 (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Club'), (SELECT ID FROM Restaurantes WHERE Nombre = 'Vinos del Sur'), (SELECT ID FROM Usuarios WHERE NombreUsuario = 'fran')),
('Cena con amigos', DATEADD(day, 7, @hoy), DATEADD(day, 7, @hoy), 0,
 (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Random'), NULL, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'martinag'));
GO

-- PRUEBA: lugares propuestos en cada plan
INSERT INTO PlanesRestaurantes (idPlan, idRestaurante)
SELECT p.ID, r.ID FROM Planes p, Restaurantes r
WHERE (p.Titulo = 'Cena de cumpleaños' AND r.Nombre IN ('Bodega Los Alamos', 'Vinos del Sur', 'Asador Don Pepe'))
   OR (p.Titulo = 'Salida de vinos' AND r.Nombre IN ('Vinos del Sur', 'La Bodega del Puerto', 'El Rincón Criollo'))
   OR (p.Titulo = 'Cena con amigos' AND r.Nombre IN ('Trattoria Nonna', 'Cantina Vecchia', 'Sushi Bar Nami'));
GO

-- PRUEBA: horarios para votar de cada plan (los de 07 · Chat de grupo).
-- PlanesHorarios va por lugar, así que cada horario se guarda una vez por cada lugar del plan.
INSERT INTO PlanesHorarios (idPlan, idRestaurante, FechaHoraPropuesta)
SELECT pr.idPlan, pr.idRestaurante, CAST(CAST(p.FechaSalida AS date) AS datetime) + CAST(h.Horario AS datetime)
FROM PlanesRestaurantes pr
JOIN Planes p ON p.ID = pr.idPlan
JOIN (VALUES ('Cena de cumpleaños', '20:30'), ('Cena de cumpleaños', '21:00'), ('Cena de cumpleaños', '21:30'),
             ('Salida de vinos', '20:00'), ('Salida de vinos', '20:30'),
             ('Cena con amigos', '21:30'), ('Cena con amigos', '22:00')) h(Titulo, Horario) ON h.Titulo = p.Titulo;
GO

-- PRUEBA: votos. Cada voto lleva el lugar y el horario (la fila de PlanesHorarios de ese lugar).
-- En Cena de cumpleaños falta votar Julián: "3 de 4 votaron".
INSERT INTO Votos (idUsuario, idPlan, idRestaurante, idHorarioVotado)
SELECT u.ID, p.ID, r.ID, ph.ID
FROM (VALUES ('Cena de cumpleaños', 'Bodega Los Alamos', '21:00', 'cami'),
             ('Cena de cumpleaños', 'Bodega Los Alamos', '21:00', 'sofi'),
             ('Cena de cumpleaños', 'Bodega Los Alamos', '21:00', 'camilaf'),
             ('Salida de vinos', 'Vinos del Sur', '20:00', 'camilaf'),
             ('Salida de vinos', 'Vinos del Sur', '20:00', 'fran'),
             ('Salida de vinos', 'Vinos del Sur', '20:00', 'tomas'),
             ('Salida de vinos', 'La Bodega del Puerto', '20:30', 'martinag'),
             ('Cena con amigos', 'Trattoria Nonna', '21:30', 'nicor')) v(Titulo, Lugar, Horario, Usuario)
JOIN Planes p ON p.Titulo = v.Titulo
JOIN Restaurantes r ON r.Nombre = v.Lugar
JOIN Usuarios u ON u.NombreUsuario = v.Usuario
JOIN PlanesHorarios ph ON ph.idPlan = p.ID AND ph.idRestaurante = r.ID
                      AND CAST(ph.FechaHoraPropuesta AS time) = CAST(v.Horario AS time);
GO

-- PRUEBA: mensajes de los chats
DECLARE @hoy datetime = CAST(CAST(GETDATE() AS date) AS datetime);
DECLARE @colegio int = (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Colegio');
DECLARE @club int = (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Club');
DECLARE @random int = (SELECT ID FROM Grupos WHERE Nombre = 'Grupo Random');

-- La encuesta de cada plan es un mensaje con idPlan (Contenido lleva el nombre del plan).
-- El chat los muestra en el orden en que se insertan, por eso van en orden de hora.
INSERT INTO Mensajes (Contenido, FechaHora, idGrupo, idUsuario, idPlan) VALUES
('Cena con amigos', DATEADD(minute, 20*60, DATEADD(day, -1, @hoy)), @random, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'martinag'), (SELECT ID FROM Planes WHERE Titulo = 'Cena con amigos')),
('Me encanta la trattoria, ¡voten!', DATEADD(minute, 20*60+10, DATEADD(day, -1, @hoy)), @random, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'martinag'), NULL),
('Salida de vinos', DATEADD(minute, 16*60, @hoy), @club, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'fran'), (SELECT ID FROM Planes WHERE Titulo = 'Salida de vinos')),
('¡Confirmado! Nos vemos el sábado.', DATEADD(minute, 17*60+15, @hoy), @club, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'fran'), NULL),
('¿Arrancamos el plan para el cumple de Sofi? Les mando opciones.', DATEADD(minute, 18*60+30, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'cami'), NULL),
('Cena de cumpleaños', DATEADD(minute, 18*60+32, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'cami'), (SELECT ID FROM Planes WHERE Titulo = 'Cena de cumpleaños')),
('¡Dale! Yo voté por la bodega, tiene opciones sin TACC.', DATEADD(minute, 18*60+36, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'sofi'), NULL),
('Yo también voté Bodega Los Alamos. ¡Faltan dos!', DATEADD(minute, 18*60+42, @hoy), @colegio, (SELECT ID FROM Usuarios WHERE NombreUsuario = 'camilaf'), NULL);

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
-- DELETE FROM PlanesHorarios WHERE idPlan IN (SELECT ID FROM Planes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random')));
-- DELETE FROM PlanesRestaurantes WHERE idPlan IN (SELECT ID FROM Planes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random')));
-- DELETE FROM Planes WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM UsuariosGrupos WHERE idGrupo IN (SELECT ID FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random'));
-- DELETE FROM Grupos WHERE Nombre IN ('Grupo Colegio','Grupo Club','Grupo Random');
-- DELETE FROM Reseñas WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Favoritos WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Busquedas WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM RestriccionesUsuarios WHERE idUsuario IN (SELECT ID FROM Usuarios WHERE Email LIKE '%@prueba.com');
-- DELETE FROM Usuarios WHERE Email LIKE '%@prueba.com';
-- DELETE FROM RestriccionesRestaurantes WHERE idRestaurante IN (SELECT ID FROM Restaurantes WHERE Nombre IN ('Bodega Los Alamos','El Rincón Criollo','Vinos del Sur','Trattoria Nonna','Asador Don Pepe','La Bodega del Puerto','Sushi Bar Nami','Cantina Vecchia'));
-- DELETE FROM Restaurantes WHERE Nombre IN ('Bodega Los Alamos','El Rincón Criollo','Vinos del Sur','Trattoria Nonna','Asador Don Pepe','La Bodega del Puerto','Sushi Bar Nami','Cantina Vecchia');
-- DELETE FROM Restricciones WHERE Nombre IN ('Sin TACC','Vegetariano','Vegano','Sin lactosa','Kosher');
-- DELETE FROM Barrios WHERE Nombre IN ('Cañuelas, Bs. As.','Palermo, CABA','San Telmo, CABA','Belgrano, CABA','Recoleta, CABA','Puerto Madero, CABA','Núñez, CABA','Caballito, CABA');
-- DELETE FROM Categorias WHERE Nombre IN ('Vinoteca','Parrilla','Bar de vinos','Italiana','Japonesa');
