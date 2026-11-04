/* =========================================================
   Bodeglook - Datos de prueba (solo INSERT)
   Basados en el Figma (Prototipo Alta). Base vacía.
   Usuaria principal: Camila Fernández (camilaf)
   ========================================================= */

USE Bodeglook;
GO

/* ---------- 1) Barrios (11) ---------- */

INSERT INTO Barrios (Nombre, Latitud, Longitud) VALUES
(N'Palermo',       -34.588900, -58.430600),
(N'Recoleta',      -34.587500, -58.397400),
(N'Belgrano',      -34.562700, -58.458300),
(N'San Telmo',     -34.621800, -58.373100),
(N'Caballito',     -34.618600, -58.441700),
(N'Villa Crespo',  -34.599300, -58.438000),
(N'Almagro',       -34.609200, -58.420600),
(N'Núñez',         -34.544500, -58.463700),
(N'Puerto Madero', -34.611800, -58.363600),
(N'Colegiales',    -34.574000, -58.449000),
(N'Cañuelas',      -35.051700, -58.760600);

/* ---------- Categorías (12) ---------- */

INSERT INTO Categorias (Nombre) VALUES
(N'Vinoteca'), (N'Bar de vinos'), (N'Parrilla'), (N'Japonesa'), (N'Italiana'), (N'Vegana'),
(N'Bodegón'), (N'Brunch'), (N'Árabe'), (N'Cervecería'), (N'Pizzería'), (N'Heladería');

/* ---------- Restricciones (10) ---------- */

INSERT INTO Restricciones (Nombre) VALUES
(N'Sin TACC'), (N'Vegetariano'), (N'Vegano'), (N'Sin lactosa'), (N'Sin frutos secos'),
(N'Sin mariscos'), (N'Kosher'), (N'Halal'), (N'Sin huevo'), (N'Sin cerdo');


/* ---------- 2) Usuarios (10) ---------- */

INSERT INTO Usuarios (NombreCompleto, NombreUsuario, Email, Contraseña, FotoPerfil, Biografía,
                      FechaRegistro, idBarrio, Frecuencia, SoloApto)
SELECT v.Nombre, v.Usuario, v.Email, N'Bodeglook2026', NULL, v.Bio, v.Fecha, B.ID, v.Frec, v.Solo
FROM (VALUES
    (N'Camila Fernández', 'camilaf',     'camila.fernandez@example.com', N'Vegetariana, fan de las vinotecas y de armar planes con amigos.', '2026-02-01T10:00:00', N'Palermo',       'Una vez por semana',      0),
    (N'Julián Pérez',     'julianperez', 'julian.perez@example.com',     N'Malbec y tabla de quesos, siempre.',                              '2026-02-03T19:20:00', N'Recoleta',      'Varias veces por semana', 0),
    (N'Sofi Ortega',      'sofiortega',  'sofi.ortega@example.com',      N'Celíaca: busco lugares sin TACC de verdad.',                      '2026-02-05T21:15:00', N'San Telmo',     'Una vez por semana',      1),
    (N'Fran Molina',      'franmolina',  'fran.molina@example.com',      N'Sushi los jueves, asado los domingos.',                           '2026-02-08T12:40:00', N'Belgrano',      'Cada tanto',              0),
    (N'Martina Giménez',  'martinag',    'martina.gimenez@example.com',  N'Brunch lover y cazadora de cafés lindos.',                        '2026-02-11T09:05:00', N'Caballito',     'Una vez por semana',      0),
    (N'Nico Ramos',       'nicoramos',   'nico.ramos@example.com',       N'After office y cervezas artesanales.',                            '2026-03-01T18:30:00', N'Villa Crespo',  'Varias veces por semana', 0),
    (N'Lucía Paz',        'lucipaz',     'lucia.paz@example.com',        N'Pruebo un lugar nuevo cada semana.',                              '2026-03-10T16:00:00', N'Almagro',       'Una vez por semana',      0),
    (N'Lucía Benítez',    'lucib',       'lucia.benitez@example.com',    N'Vegana hace cinco años.',                                         '2026-03-15T11:25:00', N'Núñez',         'Cada tanto',              1),
    (N'Tomás Aguirre',    'tomiaguirre', 'tomas.aguirre@example.com',    N'Fútbol los martes y después tercer tiempo.',                      '2026-04-02T22:10:00', N'Colegiales',    'Una vez por semana',      0),
    (N'Delfina Castro',   'delficastro', 'delfina.castro@example.com',   N'Organizo el viaje a Mendoza, pregúntenme por bodegas.',           '2026-04-20T14:45:00', N'Puerto Madero', 'Cada tanto',              0)
) v(Nombre, Usuario, Email, Bio, Fecha, Barrio, Frec, Solo)
INNER JOIN Barrios B ON B.Nombre = v.Barrio;


/* ---------- 3) Restaurantes (12) — los 5 primeros salen del Figma ---------- */

INSERT INTO Restaurantes (Nombre, DireccionCalle, DireccionNumero, DireccionCodigoPostal, Descripcion,
                          Telefono, HorarioApertura, HorarioCerrado, RangoPrecio, Foto, idBarrio, idCategoria,
                          EsPro, SitioWeb, IdGeoapify, Latitud, Longitud, Origen)
SELECT v.Nombre, v.Calle, v.Numero, v.CP, v.Descr, v.Tel, v.Abre, v.Cierra, v.Precio, NULL, B.ID, C.ID,
       v.Pro, v.Web, NULL, v.Lat, v.Lon, 'Manual'
FROM (VALUES
    (N'Bodega Los Alamos',    N'Av. Libertad',      1250, 1814, N'Bodega boutique con salón de degustación y patio al aire libre. Especialidad en vinos de la región y maridajes con tabla de quesos.', '+54 11 5555-1234', '1900-01-01T12:00:00', '1900-01-01T23:00:00', 3, N'Cañuelas',      N'Vinoteca',     1, 'https://ejemplo.com/bodega-los-alamos', -35.050900, -58.758200),
    (N'Vinos del Sur',        N'Defensa',           1020, 1065, N'Bar de vinos con más de 200 etiquetas y picadas para compartir.',                                                            '+54 11 4362-1020', '1900-01-01T18:00:00', '1900-01-01T23:30:00', 3, N'San Telmo',     N'Bar de vinos', 1, 'https://ejemplo.com/vinos-del-sur',     -34.620500, -58.371800),
    (N'Asador Don Pepe',      N'Av. Pueyrredón',    1720, 1119, N'Parrilla de barrio con cortes a la leña y provoleta.',                                                                       '+54 11 4823-1720', '1900-01-01T12:00:00', '1900-01-01T23:30:00', 3, N'Recoleta',      N'Parrilla',     0, NULL,                                    -34.590600, -58.398600),
    (N'Sushi Bar Nami',       N'Juramento',         2380, 1428, N'Barra de sushi omakase y rolls de autor.',                                                                                   '+54 11 4783-2380', '1900-01-01T19:30:00', '1900-01-01T23:30:00', 4, N'Belgrano',      N'Japonesa',     1, 'https://ejemplo.com/sushi-bar-nami',    -34.561800, -58.456900),
    (N'Trattoria Bellini',    N'Gorriti',           4850, 1414, N'Cocina italiana de trattoria: risottos, pastas frescas y tiramisú.',                                                         '+54 11 4831-4850', '1900-01-01T12:00:00', '1900-01-01T23:30:00', 3, N'Palermo',       N'Italiana',     0, NULL,                                    -34.590100, -58.429200),
    (N'La Huerta de Almagro', N'Bulnes',            850,  1176, N'Cocina 100% vegana con ingredientes de huerta propia.',                                                                      '+54 11 4862-0850', '1900-01-01T10:00:00', '1900-01-01T22:00:00', 2, N'Almagro',       N'Vegana',       0, NULL,                                    -34.604300, -58.418800),
    (N'Bodegón El Pibe',      N'José María Moreno', 350,  1424, N'Bodegón porteño: milanesas, tortilla y flan casero.',                                                                        '+54 11 4901-0350', '1900-01-01T12:00:00', '1900-01-01T23:30:00', 2, N'Caballito',     N'Bodegón',      0, NULL,                                    -34.620900, -58.434100),
    (N'Café Paradiso',        N'Zapiola',           1500, 1426, N'Brunch todo el día, café de especialidad y pastelería sin TACC.',                                                            '+54 11 4552-1500', '1900-01-01T08:30:00', '1900-01-01T19:00:00', 2, N'Colegiales',    N'Brunch',       0, NULL,                                    -34.573100, -58.451200),
    (N'Shawarma Beirut',      N'Scalabrini Ortiz',  600,  1414, N'Shawarma, falafel y hummus de receta libanesa.',                                                                             '+54 11 4857-0600', '1900-01-01T12:00:00', '1900-01-01T23:00:00', 1, N'Villa Crespo',  N'Árabe',        0, NULL,                                    -34.598700, -58.436100),
    (N'Cervecería Lúpulo',    N'Honduras',          5700, 1414, N'Cervecería artesanal con hamburguesas y papas con cheddar.',                                                                 '+54 11 4776-5700', '1900-01-01T17:00:00', '1900-01-01T23:59:00', 2, N'Palermo',       N'Cervecería',   0, NULL,                                    -34.582900, -58.432700),
    (N'Pizzería La Fugazza',  N'Av. Corrientes',    4120, 1195, N'Pizza al molde y fugazzeta rellena, como en los 80.',                                                                        '+54 11 4867-4120', '1900-01-01T12:00:00', '1900-01-01T23:59:00', 1, N'Almagro',       N'Pizzería',     0, NULL,                                    -34.603500, -58.421000),
    (N'Heladería Nieve Pura', N'Av. Santa Fe',      1980, 1123, N'Helado artesanal con sabores veganos y sin TACC.',                                                                           '+54 11 4811-1980', '1900-01-01T11:00:00', '1900-01-01T23:30:00', 1, N'Recoleta',      N'Heladería',    0, NULL,                                    -34.595200, -58.396000)
) v(Nombre, Calle, Numero, CP, Descr, Tel, Abre, Cierra, Precio, Barrio, Categoria, Pro, Web, Lat, Lon)
INNER JOIN Barrios B    ON B.Nombre = v.Barrio
INNER JOIN Categorias C ON C.Nombre = v.Categoria;


/* ---------- 4) Cuentas de restaurantes (12) ---------- */

INSERT INTO CuentasRestaurantes (Email, Contraseña, FechaRegistro, idRestaurante)
SELECT v.Email, N'Bodeglook2026', v.Fecha, R.ID
FROM (VALUES
    ('contacto@bodegalosalamos.example.com', '2026-05-02T10:00:00', N'Bodega Los Alamos'),
    ('hola@vinosdelsur.example.com',         '2026-05-10T11:30:00', N'Vinos del Sur'),
    ('reservas@asadordonpepe.example.com',   '2026-05-18T12:15:00', N'Asador Don Pepe'),
    ('info@sushibarnami.example.com',        '2026-06-01T09:40:00', N'Sushi Bar Nami'),
    ('hola@trattoriabellini.example.com',    '2026-06-12T16:20:00', N'Trattoria Bellini'),
    ('contacto@huertaalmagro.example.com',   '2026-06-25T10:05:00', N'La Huerta de Almagro'),
    ('elpibe@bodegonelpibe.example.com',     '2026-07-03T13:50:00', N'Bodegón El Pibe'),
    ('hola@cafeparadiso.example.com',        '2026-07-15T08:45:00', N'Café Paradiso'),
    ('info@shawarmabeirut.example.com',      '2026-07-28T17:10:00', N'Shawarma Beirut'),
    ('birra@cervecerialupulo.example.com',   '2026-08-09T18:30:00', N'Cervecería Lúpulo'),
    ('hola@lafugazza.example.com',           '2026-08-20T12:00:00', N'Pizzería La Fugazza'),
    ('info@nievepura.example.com',           '2026-09-02T15:30:00', N'Heladería Nieve Pura')
) v(Email, Fecha, Rest)
INNER JOIN Restaurantes R ON R.Nombre = v.Rest;


/* ---------- 5) Restricciones de restaurantes (17) ---------- */

INSERT INTO RestriccionesRestaurantes (idRestaurante, idRestriccion)
SELECT R.ID, X.ID
FROM (VALUES
    (N'Bodega Los Alamos', N'Sin TACC'), (N'Bodega Los Alamos', N'Vegetariano'),
    (N'Vinos del Sur', N'Vegetariano'),
    (N'Asador Don Pepe', N'Sin TACC'),
    (N'Sushi Bar Nami', N'Sin lactosa'),
    (N'Trattoria Bellini', N'Vegetariano'),
    (N'La Huerta de Almagro', N'Vegano'), (N'La Huerta de Almagro', N'Vegetariano'), (N'La Huerta de Almagro', N'Sin lactosa'),
    (N'Café Paradiso', N'Sin TACC'), (N'Café Paradiso', N'Vegetariano'),
    (N'Shawarma Beirut', N'Sin cerdo'), (N'Shawarma Beirut', N'Halal'),
    (N'Bodegón El Pibe', N'Sin frutos secos'),
    (N'Pizzería La Fugazza', N'Vegetariano'),
    (N'Heladería Nieve Pura', N'Sin TACC'), (N'Heladería Nieve Pura', N'Vegano')
) v(Rest, Restr)
INNER JOIN Restaurantes R  ON R.Nombre = v.Rest
INNER JOIN Restricciones X ON X.Nombre = v.Restr;


/* ---------- 6) Datos del onboarding: restricciones, zonas y compañías ---------- */

INSERT INTO RestriccionesUsuarios (idUsuario, idRestricciones)
SELECT U.ID, X.ID
FROM (VALUES
    ('camilaf', N'Vegetariano'), ('sofiortega', N'Sin TACC'), ('lucib', N'Vegano'), ('lucib', N'Sin lactosa'),
    ('martinag', N'Sin lactosa'), ('nicoramos', N'Sin cerdo'), ('delficastro', N'Kosher'),
    ('tomiaguirre', N'Sin mariscos'), ('franmolina', N'Sin frutos secos'), ('lucipaz', N'Vegetariano')
) v(Usuario, Restr)
INNER JOIN Usuarios U      ON U.NombreUsuario = v.Usuario
INNER JOIN Restricciones X ON X.Nombre = v.Restr;

INSERT INTO ZonasUsuarios (idUsuario, idBarrio)
SELECT U.ID, B.ID
FROM (VALUES
    ('camilaf', N'Palermo'), ('camilaf', N'Recoleta'), ('camilaf', N'San Telmo'),
    ('julianperez', N'Recoleta'), ('julianperez', N'Palermo'),
    ('sofiortega', N'San Telmo'), ('sofiortega', N'Puerto Madero'),
    ('franmolina', N'Belgrano'), ('franmolina', N'Núñez'),
    ('martinag', N'Caballito'), ('martinag', N'Almagro'),
    ('nicoramos', N'Villa Crespo'),
    ('lucib', N'Núñez'), ('lucib', N'Colegiales')
) v(Usuario, Barrio)
INNER JOIN Usuarios U ON U.NombreUsuario = v.Usuario
INNER JOIN Barrios B  ON B.Nombre = v.Barrio;

INSERT INTO CompaniasUsuarios (idUsuario, Compania)
SELECT U.ID, v.Compania
FROM (VALUES
    ('camilaf', 'Amigos'), ('camilaf', 'Pareja'),
    ('julianperez', 'Amigos'), ('julianperez', 'Compañeros de trabajo'),
    ('sofiortega', 'Amigos'), ('sofiortega', 'Familia'),
    ('franmolina', 'Amigos'),
    ('martinag', 'Amigos'), ('martinag', 'Familia'),
    ('nicoramos', 'Compañeros de trabajo'),
    ('lucipaz', 'Pareja'), ('lucib', 'Amigos'),
    ('tomiaguirre', 'Familia'), ('delficastro', 'Pareja')
) v(Usuario, Compania)
INNER JOIN Usuarios U ON U.NombreUsuario = v.Usuario;


/* ---------- 7) Búsquedas recientes (11) ---------- */

INSERT INTO Busquedas (idUsuario, Texto, Fecha)
SELECT U.ID, v.Texto, v.Fecha
FROM (VALUES
    ('camilaf', N'Bodega Los Alamos',    '2026-11-03T20:10:00'),
    ('camilaf', N'Sushi Bar Nami',       '2026-11-03T20:12:00'),
    ('camilaf', N'Parrillas en Palermo', '2026-11-03T20:15:00'),
    ('camilaf', N'vinoteca',             '2026-11-01T18:40:00'),
    ('camilaf', N'sin tacc',             '2026-10-19T17:05:00'),
    ('sofiortega', N'sin tacc san telmo', '2026-10-18T13:30:00'),
    ('sofiortega', N'Vinos del Sur',      '2026-10-18T13:32:00'),
    ('julianperez', N'parrilla recoleta', '2026-10-30T21:00:00'),
    ('lucib', N'vegano',                  '2026-10-28T12:20:00'),
    ('lucib', N'La Huerta de Almagro',    '2026-10-28T12:22:00'),
    ('nicoramos', N'shawarma',            '2026-10-31T19:45:00')
) v(Usuario, Texto, Fecha)
INNER JOIN Usuarios U ON U.NombreUsuario = v.Usuario;


/* ---------- 8) Favoritos (11) — los 4 primeros son "Tus favoritos" de Camila ---------- */

INSERT INTO Favoritos (idRestaurante, idUsuario)
SELECT R.ID, U.ID
FROM (VALUES
    ('camilaf', N'Sushi Bar Nami'), ('camilaf', N'Bodega Los Alamos'),
    ('camilaf', N'Trattoria Bellini'), ('camilaf', N'Café Paradiso'),
    ('julianperez', N'Bodega Los Alamos'), ('julianperez', N'Asador Don Pepe'),
    ('sofiortega', N'Café Paradiso'), ('sofiortega', N'Vinos del Sur'),
    ('lucib', N'La Huerta de Almagro'), ('nicoramos', N'Shawarma Beirut'),
    ('franmolina', N'Cervecería Lúpulo')
) v(Usuario, Rest)
INNER JOIN Usuarios U     ON U.NombreUsuario = v.Usuario
INNER JOIN Restaurantes R ON R.Nombre = v.Rest;


/* ---------- 9) Reseñas (47) — Camila tiene 12, como en su perfil del Figma.
   Promedios del Figma: Bodega Los Alamos 4.7 · Vinos del Sur 4.8 · Asador Don Pepe 4.5
   Sushi Bar Nami 4.9 · Trattoria Bellini 4.3 · Café Paradiso 4.6 ---------- */

INSERT INTO [Reseñas] (Puntaje, Comentario, Foto, Fecha, idUsuario, idRestaurante)
SELECT v.Puntaje, v.Comentario, NULL, v.Fecha, U.ID, R.ID
FROM (VALUES
    -- Bodega Los Alamos (4.7)
    (5, N'El patio es hermoso y la tabla de quesos con el malbec fue un golazo. Atención de 10.', '2026-11-02T13:20:00', 'julianperez', N'Bodega Los Alamos'),
    (5, N'Muy buena opción sin TACC, nos cuidaron mucho. Recomiendo ir temprano.',               '2026-10-26T11:00:00', 'sofiortega',  N'Bodega Los Alamos'),
    (4, N'Fuimos por el cumple de Sofi. Lindo lugar, opciones vegetarianas ricas, un poco lejos.', '2026-10-25T10:30:00', 'camilaf',     N'Bodega Los Alamos'),
    -- Vinos del Sur (4.8)
    (5, N'La carta de vinos es enorme y te recomiendan bien.',            '2026-09-14T23:10:00', 'camilaf',   N'Vinos del Sur'),
    (5, N'Picada abundante y buena música. Volvemos seguro.',             '2026-09-20T22:45:00', 'franmolina', N'Vinos del Sur'),
    (5, N'Ideal para ir con amigas un jueves.',                           '2026-09-25T21:30:00', 'martinag',  N'Vinos del Sur'),
    (5, N'Los vinos naranjos, una sorpresa.',                             '2026-10-02T23:00:00', 'nicoramos', N'Vinos del Sur'),
    (4, N'Muy lindo, pero un poco ruidoso el sábado.',                    '2026-10-11T00:15:00', 'lucipaz',   N'Vinos del Sur'),
    -- Asador Don Pepe (4.5)
    (4, N'Para no comer carne está la provoleta y las verduras a la leña, bien hechas.', '2026-08-16T22:00:00', 'camilaf',     N'Asador Don Pepe'),
    (5, N'El vacío, de los mejores de Recoleta.',                                       '2026-08-30T14:10:00', 'julianperez', N'Asador Don Pepe'),
    -- Sushi Bar Nami (4.9)
    (5, N'El omakase vale cada peso. Tienen rolls veggie muy buenos.', '2026-07-10T22:30:00', 'camilaf',     N'Sushi Bar Nami'),
    (5, N'Pescado fresquísimo.',                                       '2026-07-17T22:00:00', 'franmolina',  N'Sushi Bar Nami'),
    (5, N'Atención impecable en la barra.',                            '2026-07-24T21:40:00', 'martinag',    N'Sushi Bar Nami'),
    (5, N'El mejor sushi de Belgrano.',                                '2026-08-07T23:05:00', 'nicoramos',   N'Sushi Bar Nami'),
    (5, N'Me explicaron cada pieza, una experiencia.',                 '2026-08-14T22:20:00', 'lucipaz',     N'Sushi Bar Nami'),
    (5, N'Tuvieron opciones veganas sin que las pidiera dos veces.',   '2026-08-21T21:50:00', 'lucib',       N'Sushi Bar Nami'),
    (5, N'Fui por un cumple y nos atendieron re bien.',                '2026-08-28T22:35:00', 'julianperez', N'Sushi Bar Nami'),
    (5, N'Avisé que era celíaca y usaron salsa sin TACC.',             '2026-09-04T21:15:00', 'sofiortega',  N'Sushi Bar Nami'),
    (5, N'Caro pero lo vale.',                                         '2026-09-11T22:50:00', 'tomiaguirre', N'Sushi Bar Nami'),
    (4, N'Muy rico, aunque hubo que esperar mesa.',                    '2026-09-18T23:30:00', 'delficastro', N'Sushi Bar Nami'),
    -- Trattoria Bellini (4.3)
    (4, N'El risotto de hongos, buenísimo. El servicio algo lento.', '2026-10-09T23:00:00', 'camilaf',   N'Trattoria Bellini'),
    (5, N'El tiramisú es de otro planeta.',                          '2026-10-12T22:15:00', 'martinag',  N'Trattoria Bellini'),
    (4, N'Pastas frescas de verdad, porciones justas.',              '2026-10-16T21:40:00', 'nicoramos', N'Trattoria Bellini'),
    -- La Huerta de Almagro
    (4, N'Todo vegano y nadie lo extrañó. El budín de banana, top.', '2026-09-27T14:00:00', 'camilaf', N'La Huerta de Almagro'),
    (5, N'Por fin un lugar donde puedo pedir cualquier cosa.',       '2026-10-04T13:30:00', 'lucib',   N'La Huerta de Almagro'),
    (4, N'Rico y sano, el local es chiquito.',                       '2026-10-08T20:45:00', 'lucipaz', N'La Huerta de Almagro'),
    -- Bodegón El Pibe
    (4, N'La tortilla babé es un clásico. Para vegetarianos hay pocas opciones.', '2026-09-19T23:30:00', 'camilaf',    N'Bodegón El Pibe'),
    (4, N'Milanesa enorme, se comparte.',                                         '2026-09-19T23:45:00', 'franmolina', N'Bodegón El Pibe'),
    (5, N'El flan con dulce de leche, como el de la abuela.',                     '2026-09-20T00:10:00', 'nicoramos',  N'Bodegón El Pibe'),
    -- Café Paradiso (4.6)
    (5, N'El brunch del cumple de Martina fue un éxito. Hay medialunas sin TACC.', '2026-09-27T13:00:00', 'camilaf',     N'Café Paradiso'),
    (5, N'Pastelería sin TACC que no parece sin TACC.',                            '2026-09-27T13:20:00', 'sofiortega',  N'Café Paradiso'),
    (4, N'Café rico, se llena mucho los domingos.',                                '2026-10-05T12:30:00', 'delficastro', N'Café Paradiso'),
    (4, N'Buen tostado de palta, faltan opciones veganas dulces.',                 '2026-10-12T11:45:00', 'lucib',       N'Café Paradiso'),
    (5, N'Lindo para ir a trabajar un rato.',                                      '2026-10-19T10:15:00', 'tomiaguirre', N'Café Paradiso'),
    -- Shawarma Beirut
    (4, N'El falafel está muy bien condimentado.',   '2026-10-10T00:20:00', 'camilaf',    N'Shawarma Beirut'),
    (5, N'Shawarma barato y abundante.',             '2026-10-14T21:00:00', 'nicoramos',  N'Shawarma Beirut'),
    (4, N'El hummus, de lo mejor de Villa Crespo.',  '2026-10-21T20:30:00', 'franmolina', N'Shawarma Beirut'),
    -- Cervecería Lúpulo
    (3, N'Buena birra, pero para comer veggie solo hay papas.', '2026-10-10T01:00:00', 'camilaf',     N'Cervecería Lúpulo'),
    (4, N'Ideal para el tercer tiempo.',                        '2026-10-28T00:30:00', 'tomiaguirre', N'Cervecería Lúpulo'),
    (4, N'La IPA de la casa está muy bien.',                    '2026-10-28T00:45:00', 'franmolina',  N'Cervecería Lúpulo'),
    (3, N'Las hamburguesas llegaron tibias.',                   '2026-10-28T01:00:00', 'nicoramos',   N'Cervecería Lúpulo'),
    -- Pizzería La Fugazza
    (4, N'La fugazzeta rellena, un clásico. Hay opción vegetariana.', '2026-08-22T22:30:00', 'camilaf',   N'Pizzería La Fugazza'),
    (4, N'Barata y abundante, ideal después del cine.',               '2026-08-29T23:10:00', 'martinag',  N'Pizzería La Fugazza'),
    (5, N'La mejor pizza al molde de Almagro.',                       '2026-09-05T22:40:00', 'nicoramos', N'Pizzería La Fugazza'),
    -- Heladería Nieve Pura
    (5, N'Tienen sabores veganos y el de pistacho es increíble.',     '2026-10-04T17:30:00', 'camilaf',    N'Heladería Nieve Pura'),
    (5, N'Cucurucho sin TACC, ¡por fin!',                             '2026-10-11T18:15:00', 'sofiortega', N'Heladería Nieve Pura'),
    (4, N'Rico, aunque los veganos se terminan rápido.',              '2026-10-18T19:00:00', 'lucib',      N'Heladería Nieve Pura')
) v(Puntaje, Comentario, Fecha, Usuario, Rest)
INNER JOIN Usuarios U     ON U.NombreUsuario = v.Usuario
INNER JOIN Restaurantes R ON R.Nombre = v.Rest;


/* ---------- 10) Grupos (10) y sus integrantes (34) ---------- */

INSERT INTO Grupos (Nombre, Foto, FechaCreacion)
VALUES (N'Grupo Colegio',        NULL, '2026-02-10T18:00:00'),
       (N'Amigas de la facu',    NULL, '2026-03-05T20:30:00'),
       (N'After office',         NULL, '2026-04-18T19:00:00'),
       (N'Los del vino',         NULL, '2026-05-02T21:10:00'),
       (N'Club del sushi',       NULL, '2026-05-20T22:00:00'),
       (N'Brunch de domingo',    NULL, '2026-06-07T10:30:00'),
       (N'Familia Ortega',       NULL, '2026-06-15T13:00:00'),
       (N'Fútbol de los martes', NULL, '2026-07-01T23:00:00'),
       (N'Viaje a Mendoza',      NULL, '2026-08-12T17:45:00'),
       (N'Cumple de Fran',       NULL, '2026-09-01T16:20:00');

INSERT INTO UsuariosGrupos (idUsuario, idGrupo, UltimaLectura)
SELECT U.ID, G.ID, v.Lectura
FROM (VALUES
    ('camilaf', N'Grupo Colegio', '2026-10-20T18:42:00'), ('sofiortega', N'Grupo Colegio', NULL),
    ('franmolina', N'Grupo Colegio', NULL), ('julianperez', N'Grupo Colegio', NULL),
    ('camilaf', N'Amigas de la facu', NULL), ('martinag', N'Amigas de la facu', NULL),
    ('lucipaz', N'Amigas de la facu', NULL), ('sofiortega', N'Amigas de la facu', NULL),
    ('camilaf', N'After office', NULL), ('nicoramos', N'After office', NULL),
    ('julianperez', N'After office', NULL), ('lucib', N'After office', NULL),
    ('camilaf', N'Los del vino', NULL), ('julianperez', N'Los del vino', NULL), ('delficastro', N'Los del vino', NULL),
    ('camilaf', N'Club del sushi', NULL), ('franmolina', N'Club del sushi', NULL), ('martinag', N'Club del sushi', NULL),
    ('sofiortega', N'Brunch de domingo', NULL), ('lucib', N'Brunch de domingo', NULL),
    ('tomiaguirre', N'Brunch de domingo', NULL), ('delficastro', N'Brunch de domingo', NULL),
    ('sofiortega', N'Familia Ortega', NULL), ('tomiaguirre', N'Familia Ortega', NULL),
    ('franmolina', N'Fútbol de los martes', NULL), ('nicoramos', N'Fútbol de los martes', NULL), ('tomiaguirre', N'Fútbol de los martes', NULL),
    ('julianperez', N'Viaje a Mendoza', NULL), ('delficastro', N'Viaje a Mendoza', NULL), ('martinag', N'Viaje a Mendoza', NULL),
    ('franmolina', N'Cumple de Fran', NULL), ('camilaf', N'Cumple de Fran', NULL),
    ('sofiortega', N'Cumple de Fran', NULL), ('nicoramos', N'Cumple de Fran', NULL)
) v(Usuario, Grupo, Lectura)
INNER JOIN Usuarios U ON U.NombreUsuario = v.Usuario
INNER JOIN Grupos G   ON G.Nombre = v.Grupo;


/* ---------- 11) Planes (10)
   Estado: 0 = en votación, 1 = confirmado (igual que su SP CrearPlan)
   Camila está en 7 planes y 2 están en votación, como en el Inicio del Figma ---------- */

INSERT INTO Planes (Titulo, FechaCierreVotacion, FechaSalida, Estado, idGrupo, idRestauranteElegido, idUsuarioCreador)
SELECT v.Titulo, v.Cierre, v.Salida, v.Estado, G.ID, R.ID, U.ID
FROM (VALUES
    (N'Cena de cumpleaños de Sofi', '2026-10-23T20:00:00', '2026-10-24T21:00:00', 1, N'Grupo Colegio',        N'Bodega Los Alamos', 'camilaf'),
    (N'Asado de fin de año',        '2026-12-10T20:00:00', '2026-12-12T21:00:00', 0, N'Grupo Colegio',        NULL,                 'julianperez'),
    (N'Sushi de los jueves',        '2026-11-11T18:00:00', '2026-11-12T21:00:00', 0, N'Club del sushi',       NULL,                 'franmolina'),
    (N'Brunch de cumple de Martina','2026-09-26T20:00:00', '2026-09-27T11:30:00', 1, N'Amigas de la facu',    N'Café Paradiso',     'martinag'),
    (N'After del viernes',          '2026-10-08T18:00:00', '2026-10-09T20:00:00', 1, N'After office',         N'Cervecería Lúpulo', 'nicoramos'),
    (N'Degustación en Cañuelas',    '2026-08-27T20:00:00', '2026-08-29T13:00:00', 1, N'Los del vino',         N'Bodega Los Alamos', 'julianperez'),
    (N'Cena del cumple de Fran',    '2026-09-17T20:00:00', '2026-09-19T21:00:00', 1, N'Cumple de Fran',       N'Bodegón El Pibe',   'sofiortega'),
    (N'Brunch dominguero',          '2026-11-13T20:00:00', '2026-11-15T11:00:00', 0, N'Brunch de domingo',    NULL,                 'delficastro'),
    (N'Tercer tiempo',              '2026-10-26T18:00:00', '2026-10-27T21:30:00', 1, N'Fútbol de los martes', N'Cervecería Lúpulo', 'tomiaguirre'),
    (N'Previa del viaje a Mendoza', '2026-11-18T20:00:00', '2026-11-20T20:30:00', 0, N'Viaje a Mendoza',      NULL,                 'delficastro')
) v(Titulo, Cierre, Salida, Estado, Grupo, Elegido, Creador)
INNER JOIN Grupos G     ON G.Nombre = v.Grupo
INNER JOIN Usuarios U   ON U.NombreUsuario = v.Creador
LEFT JOIN Restaurantes R ON R.Nombre = v.Elegido;


/* ---------- 12) Lugares propuestos: 3 por plan (30) ---------- */

INSERT INTO PlanesRestaurantes (idPlan, idRestaurante)
SELECT P.ID, R.ID
FROM (VALUES
    (N'Cena de cumpleaños de Sofi', N'Bodega Los Alamos'), (N'Cena de cumpleaños de Sofi', N'Vinos del Sur'), (N'Cena de cumpleaños de Sofi', N'Asador Don Pepe'),
    (N'Asado de fin de año', N'Asador Don Pepe'), (N'Asado de fin de año', N'Bodegón El Pibe'), (N'Asado de fin de año', N'Cervecería Lúpulo'),
    (N'Sushi de los jueves', N'Sushi Bar Nami'), (N'Sushi de los jueves', N'Trattoria Bellini'), (N'Sushi de los jueves', N'La Huerta de Almagro'),
    (N'Brunch de cumple de Martina', N'Café Paradiso'), (N'Brunch de cumple de Martina', N'La Huerta de Almagro'), (N'Brunch de cumple de Martina', N'Trattoria Bellini'),
    (N'After del viernes', N'Cervecería Lúpulo'), (N'After del viernes', N'Shawarma Beirut'), (N'After del viernes', N'Bodegón El Pibe'),
    (N'Degustación en Cañuelas', N'Bodega Los Alamos'), (N'Degustación en Cañuelas', N'Vinos del Sur'), (N'Degustación en Cañuelas', N'Trattoria Bellini'),
    (N'Cena del cumple de Fran', N'Bodegón El Pibe'), (N'Cena del cumple de Fran', N'Shawarma Beirut'), (N'Cena del cumple de Fran', N'Asador Don Pepe'),
    (N'Brunch dominguero', N'Café Paradiso'), (N'Brunch dominguero', N'La Huerta de Almagro'), (N'Brunch dominguero', N'Trattoria Bellini'),
    (N'Tercer tiempo', N'Cervecería Lúpulo'), (N'Tercer tiempo', N'Bodegón El Pibe'), (N'Tercer tiempo', N'Asador Don Pepe'),
    (N'Previa del viaje a Mendoza', N'Vinos del Sur'), (N'Previa del viaje a Mendoza', N'Bodega Los Alamos'), (N'Previa del viaje a Mendoza', N'Trattoria Bellini')
) v(TituloPlan, Rest)
INNER JOIN Planes P       ON P.Titulo = v.TituloPlan
INNER JOIN Restaurantes R ON R.Nombre = v.Rest;


/* ---------- 13) Horarios: 3 por cada lugar propuesto (90)
   Media hora antes, a la hora y media hora después de la salida
   (en el Figma: 20:30, 21:00 y 21:30) ---------- */

INSERT INTO PlanesHorarios (idPlan, idRestaurante, FechaHoraPropuesta)
SELECT PR.idPlan, PR.idRestaurante, DATEADD(MINUTE, T.Minutos, P.FechaSalida)
FROM PlanesRestaurantes PR
INNER JOIN Planes P ON P.ID = PR.idPlan
CROSS JOIN (VALUES (-30), (0), (30)) T(Minutos);


/* ---------- 14) Votos (30) — lugar + horario
   Cumple de Sofi igual al Figma: Bodega 3 votos (Cami, Sofi, Julián) y Vinos del Sur 1 (Fran) ---------- */

INSERT INTO Votos (idUsuario, idPlan, idRestaurante, idHorarioVotado)
SELECT U.ID, P.ID, R.ID, PH.ID
FROM (VALUES
    ('camilaf',     N'Cena de cumpleaños de Sofi', N'Bodega Los Alamos', '2026-10-24T21:00:00'),
    ('sofiortega',  N'Cena de cumpleaños de Sofi', N'Bodega Los Alamos', '2026-10-24T21:00:00'),
    ('julianperez', N'Cena de cumpleaños de Sofi', N'Bodega Los Alamos', '2026-10-24T21:00:00'),
    ('franmolina',  N'Cena de cumpleaños de Sofi', N'Vinos del Sur',     '2026-10-24T21:30:00'),
    ('julianperez', N'Asado de fin de año', N'Asador Don Pepe', '2026-12-12T21:00:00'),
    ('camilaf',     N'Asado de fin de año', N'Asador Don Pepe', '2026-12-12T21:30:00'),
    ('sofiortega',  N'Asado de fin de año', N'Bodegón El Pibe', '2026-12-12T21:00:00'),
    ('franmolina',  N'Sushi de los jueves', N'Sushi Bar Nami', '2026-11-12T21:00:00'),
    ('martinag',    N'Sushi de los jueves', N'Sushi Bar Nami', '2026-11-12T20:30:00'),
    ('camilaf',     N'Brunch de cumple de Martina', N'Café Paradiso',        '2026-09-27T11:30:00'),
    ('martinag',    N'Brunch de cumple de Martina', N'Café Paradiso',        '2026-09-27T11:30:00'),
    ('sofiortega',  N'Brunch de cumple de Martina', N'Café Paradiso',        '2026-09-27T12:00:00'),
    ('lucipaz',     N'Brunch de cumple de Martina', N'La Huerta de Almagro', '2026-09-27T11:30:00'),
    ('nicoramos',   N'After del viernes', N'Cervecería Lúpulo', '2026-10-09T20:00:00'),
    ('julianperez', N'After del viernes', N'Cervecería Lúpulo', '2026-10-09T20:00:00'),
    ('lucib',       N'After del viernes', N'Cervecería Lúpulo', '2026-10-09T20:00:00'),
    ('camilaf',     N'After del viernes', N'Shawarma Beirut',   '2026-10-09T20:30:00'),
    ('julianperez', N'Degustación en Cañuelas', N'Bodega Los Alamos', '2026-08-29T13:00:00'),
    ('delficastro', N'Degustación en Cañuelas', N'Bodega Los Alamos', '2026-08-29T13:00:00'),
    ('camilaf',     N'Degustación en Cañuelas', N'Vinos del Sur',     '2026-08-29T13:30:00'),
    ('franmolina',  N'Cena del cumple de Fran', N'Bodegón El Pibe', '2026-09-19T21:00:00'),
    ('nicoramos',   N'Cena del cumple de Fran', N'Bodegón El Pibe', '2026-09-19T21:00:00'),
    ('camilaf',     N'Cena del cumple de Fran', N'Bodegón El Pibe', '2026-09-19T21:00:00'),
    ('sofiortega',  N'Cena del cumple de Fran', N'Shawarma Beirut', '2026-09-19T21:00:00'),
    ('sofiortega',  N'Brunch dominguero', N'Café Paradiso',        '2026-11-15T11:00:00'),
    ('lucib',       N'Brunch dominguero', N'La Huerta de Almagro', '2026-11-15T11:00:00'),
    ('tomiaguirre', N'Tercer tiempo', N'Cervecería Lúpulo', '2026-10-27T21:30:00'),
    ('franmolina',  N'Tercer tiempo', N'Cervecería Lúpulo', '2026-10-27T21:30:00'),
    ('nicoramos',   N'Tercer tiempo', N'Bodegón El Pibe',   '2026-10-27T21:00:00'),
    ('julianperez', N'Previa del viaje a Mendoza', N'Vinos del Sur', '2026-11-20T20:30:00')
) v(Usuario, TituloPlan, Rest, Hora)
INNER JOIN Usuarios U       ON U.NombreUsuario = v.Usuario
INNER JOIN Planes P         ON P.Titulo = v.TituloPlan
INNER JOIN Restaurantes R   ON R.Nombre = v.Rest
INNER JOIN PlanesHorarios PH ON PH.idPlan = P.ID AND PH.idRestaurante = R.ID
                            AND PH.FechaHoraPropuesta = CAST(v.Hora AS DATETIME);


/* ---------- 15) Mensajes (20) — el Grupo Colegio es el chat del Figma.
   Los mensajes con plan son la encuesta que aparece en el chat ---------- */

INSERT INTO Mensajes (Contenido, FechaHora, idGrupo, idUsuario, idPlan)
SELECT v.Texto, v.Fecha, G.ID, U.ID, P.ID
FROM (VALUES
    (N'¿Arrancamos el plan para el cumple de Sofi? Les mando la encuesta.', '2026-10-20T18:30:00', N'Grupo Colegio', 'camilaf',     NULL),
    (N'Cena de cumpleaños de Sofi',                                          '2026-10-20T18:31:00', N'Grupo Colegio', 'camilaf',     N'Cena de cumpleaños de Sofi'),
    (N'¡Dale! Yo voté por la bodega, tiene opciones sin TACC.',              '2026-10-20T18:36:00', N'Grupo Colegio', 'sofiortega',  NULL),
    (N'Votamos todos y gana la bodega. ¡Confirmá, Cami!',                    '2026-10-20T18:42:00', N'Grupo Colegio', 'julianperez', NULL),
    (N'Yo voté Vinos del Sur, pero me sumo igual.',                          '2026-10-20T18:45:00', N'Grupo Colegio', 'franmolina',  NULL),
    (N'Armé el plan para el asado de fin de año, voten.',                    '2026-11-02T19:00:00', N'Grupo Colegio', 'julianperez', NULL),
    (N'Asado de fin de año',                                                 '2026-11-02T19:01:00', N'Grupo Colegio', 'julianperez', N'Asado de fin de año'),
    (N'Jueves de sushi, ¿quién viene?',                                      '2026-11-03T12:10:00', N'Club del sushi', 'franmolina', N'Sushi de los jueves'),
    (N'Yo voy, pero a las 20:30 me queda mejor.',                            '2026-11-03T12:30:00', N'Club del sushi', 'martinag',   NULL),
    (N'Brunch de cumple de Martina',                                         '2026-09-20T10:00:00', N'Amigas de la facu', 'martinag', N'Brunch de cumple de Martina'),
    (N'¡Gracias por venir, chicas! El brunch estuvo increíble.',             '2026-09-27T15:00:00', N'Amigas de la facu', 'martinag', NULL),
    (N'After del viernes',                                                   '2026-10-06T17:00:00', N'After office', 'nicoramos', N'After del viernes'),
    (N'Hoy after, ¿quién se suma?',                                          '2026-10-09T17:30:00', N'After office', 'nicoramos', NULL),
    (N'Degustación en Cañuelas',                                             '2026-08-20T21:00:00', N'Los del vino', 'julianperez', N'Degustación en Cañuelas'),
    (N'Cena del cumple de Fran',                                             '2026-09-10T18:00:00', N'Cumple de Fran', 'sofiortega', N'Cena del cumple de Fran'),
    (N'Brunch dominguero',                                                   '2026-11-04T09:00:00', N'Brunch de domingo', 'delficastro', N'Brunch dominguero'),
    (N'Tercer tiempo',                                                       '2026-10-24T23:00:00', N'Fútbol de los martes', 'tomiaguirre', N'Tercer tiempo'),
    (N'Ganó la cervecería, nos vemos ahí.',                                  '2026-10-26T18:05:00', N'Fútbol de los martes', 'tomiaguirre', NULL),
    (N'Previa del viaje a Mendoza',                                          '2026-11-03T21:00:00', N'Viaje a Mendoza', 'delficastro', N'Previa del viaje a Mendoza'),
    (N'¿Almorzamos el domingo en lo de la abuela?',                          '2026-11-03T21:10:00', N'Familia Ortega', 'sofiortega', NULL)
) v(Texto, Fecha, Grupo, Usuario, TituloPlan)
INNER JOIN Grupos G   ON G.Nombre = v.Grupo
INNER JOIN Usuarios U ON U.NombreUsuario = v.Usuario
LEFT JOIN Planes P    ON P.Titulo = v.TituloPlan;
GO
