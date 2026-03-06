-- ================================================================
-- SEED DEMO COMPLETO — Control Torneos Fútbol
-- Genera ~1,800+ registros para demo/evaluación académica
-- Contraseña para TODOS los usuarios: Usuario123!
-- Hash BCrypt: $2a$12$vydnMid9NqpyoVO0knZwjutuYadLjHjzY8Ecne7MQLVI7YbaUUsuC
-- ----------------------------------------------------------------
-- INSTRUCCIONES:
--   1. Las migraciones de EF Core deben estar aplicadas primero:
--      dotnet ef database update
--   2. Ejecutar en Supabase → SQL Editor (puede dividirse en partes)
--   3. NO incluye CREATE TABLE (se manejan por EF migrations)
-- ----------------------------------------------------------------
-- RESUMEN DE DATOS:
--   1  Administrador        (admin@demo.com)
--   8  Espectadores         (espectador1..8@demo.com)
--   8  Árbitros aprobados   (arbitro1..8@demo.com)
--  24  Entrenadores         (entrenador1..24@demo.com)
--  24  Equipos              (Los Titanes, Los Rayos, ...)
-- 240  Jugadores            (jug_e{equipo}_{num}@demo.com)
--   3  Ligas, 4 Temporadas, 3 Categorías
--   6  Torneos (2 Finalizados, 3 Activos, 1 Borrador)
-- 140  Partidos distribuidos en 2024-2027
--      Marzo 2026 tiene matches de 3 torneos simultáneos
-- ================================================================

-- ========================================================
-- PARTE 0: LIMPIEZA COMPLETA
-- ADVERTENCIA: elimina TODOS los datos existentes.
-- Comentar si no se desea limpiar la DB.
-- ========================================================
TRUNCATE TABLE
    public.auditoriaaccesos,
    public.solicitudesjugadorequipo,
    public.equiposjugadorestorneos,
    public.equiposentrenadores,
    public.equipostorneos,
    public.partidos,
    public.reglaspersonalizadastorneo,
    public.configuracionestorneo,
    public.grupos,
    public.arbitros,
    public.jugadores,
    public.entrenadores,
    public.usuariosroles,
    public.personas,
    public.usuarios,
    public.torneos,
    public.equipos,
    public.temporadas,
    public.ligas,
    public.categorias
    RESTART IDENTITY CASCADE;

TRUNCATE TABLE public.roles RESTART IDENTITY CASCADE;

-- ========================================================
-- PARTE 1: TABLAS DE REFERENCIA
-- ========================================================

-- Roles
INSERT INTO public.roles (nombre, nombrenormalizado, descripcion) VALUES
    ('Administrador', 'ADMINISTRADOR', 'Administrador del sistema'),
    ('Espectador',    'ESPECTADOR',    'Usuario espectador público'),
    ('Jugador',       'JUGADOR',       'Jugador de fútbol registrado'),
    ('Entrenador',    'ENTRENADOR',    'Director técnico / entrenador'),
    ('Arbitro',       'ARBITRO',       'Árbitro certificado');

-- Ligas
INSERT INTO public.ligas (nombre, descripcion, activa, fechacreacion) VALUES
    ('Liga Nacional',   'Primera división nacional de fútbol amateur', true, NOW()),
    ('Liga Regional',   'División regional con equipos locales',       true, NOW()),
    ('Copa Interligas', 'Competencia especial entre todas las ligas',  true, NOW());

-- Temporadas
INSERT INTO public.temporadas (nombre, anio, fechainicio, fechafin, activa) VALUES
    ('Temporada 2024', 2024, '2024-01-01', '2024-12-31', false),
    ('Temporada 2025', 2025, '2025-01-01', '2025-12-31', true),
    ('Temporada 2026', 2026, '2026-01-01', '2026-12-31', true),
    ('Temporada 2027', 2027, '2027-01-01', '2027-12-31', false);

-- Categorías
INSERT INTO public.categorias (nombre, edadminima, edadmaxima, descripcion, orden) VALUES
    ('Juvenil',   16, 21, 'Categoría juvenil sub-21',  1),
    ('Amateur',   18, 45, 'Categoría amateur abierta', 2),
    ('Veteranos', 40, 65, 'Categoría veteranos 40+',   3);

-- ========================================================
-- PARTE 2: USUARIOS, PERSONAS, ENTRENADORES, ÁRBITROS,
--           JUGADORES Y EQUIPOS
-- ========================================================
DO $$
DECLARE
    v_hash TEXT      := '$2a$12$vydnMid9NqpyoVO0knZwjutuYadLjHjzY8Ecne7MQLVI7YbaUUsuC';
    v_now  TIMESTAMP := NOW();

    -- IDs de roles
    v_rol_admin      INT;
    v_rol_espectador INT;
    v_rol_jugador    INT;
    v_rol_entrenador INT;
    v_rol_arbitro    INT;

    -- Variables de trabajo
    v_uid       UUID;
    v_pid       UUID;
    v_jid       INT;
    v_eid       INT;
    v_equipo_id INT;
    i           INT;
    j           INT;

    -- Datos de nombres para generar personas variadas
    v_nombres TEXT[] := ARRAY[
        'Carlos','Luis','Miguel','José','Juan','Pedro','Roberto','Fernando',
        'Diego','Mario','Ricardo','Alejandro','Sergio','Daniel','Eduardo',
        'Raúl','Andrés','Javier','Gabriel','Ernesto','Oscar','Victor','René','Héctor','Arturo'
    ];
    v_apellidos TEXT[] := ARRAY[
        'García','Martínez','López','González','Rodríguez','Hernández','Pérez',
        'Sánchez','Ramírez','Cruz','Morales','Flores','Jiménez','Díaz','Reyes',
        'Mendoza','Vargas','Castillo','Torres','Aguilar'
    ];
    v_posiciones TEXT[] := ARRAY[
        'Portero','Defensa','Defensa','Defensa','Lateral',
        'Lateral','Mediocampista','Mediocampista','Mediocampista','Delantero','Delantero'
    ];

    -- 24 equipos con abreviaciones y sedes rotativas
    v_equipos TEXT[] := ARRAY[
        'Los Titanes','Los Rayos','Águilas Reales','Tigres FC',
        'Leones del Sur','Buitres FC','Zorros Unidos','Lobos Negros',
        'Panteras Rojas','Cóndores','Dragones FC','Fénix SC',
        'Guardianes','Atlético Central','Unión Norte','Real Sur',
        'Sporting Club','Deportivo Este','Club Oeste','Estrellas FC',
        'Halcones','Pumas SC','Rápidos FC','Olimpia'
    ];
    v_abrevs TEXT[] := ARRAY[
        'TIT','RAY','AGR','TIG','LES','BUI','ZOR','LNE',
        'PRO','CON','DRA','FEN','GUA','ACE','UNO','RSU',
        'SPC','DES','CLO','EST','HAL','PUM','RAP','OLI'
    ];
    v_sedes_equipo TEXT[] := ARRAY[
        'Estadio Nacional','Campo Norte','Cancha Central','Complejo Sur',
        'Estadio Municipal','Campo La Paz','Arena Deportiva','Cancha La Unidad'
    ];
BEGIN
    -- Obtener IDs de roles
    SELECT id INTO v_rol_admin      FROM public.roles WHERE nombre = 'Administrador';
    SELECT id INTO v_rol_espectador FROM public.roles WHERE nombre = 'Espectador';
    SELECT id INTO v_rol_jugador    FROM public.roles WHERE nombre = 'Jugador';
    SELECT id INTO v_rol_entrenador FROM public.roles WHERE nombre = 'Entrenador';
    SELECT id INTO v_rol_arbitro    FROM public.roles WHERE nombre = 'Arbitro';

    -- ── ADMINISTRADOR ──────────────────────────────────────────
    v_uid := gen_random_uuid();
    v_pid := gen_random_uuid();
    INSERT INTO public.usuarios (id,email,emailconfirmado,passwordhash,nombreusuario,
        telefonoconfirmado,autenticaciondosfactores,bloqueohabilitado,
        contadoraccesosfallidos,fechacreacion,activo)
    VALUES (v_uid,'admin@demo.com',true,v_hash,'admin',
        false,false,true,0,v_now,true);
    INSERT INTO public.personas (id,usuarioid,nombre,apellidopaterno,fechacreacion,activo)
    VALUES (v_pid,v_uid,'Administrador','Sistema',v_now,true);
    INSERT INTO public.usuariosroles (usuarioid,rolid,fechaasignacion)
    VALUES (v_uid,v_rol_admin,v_now);

    -- ── ESPECTADORES (8) ───────────────────────────────────────
    FOR i IN 1..8 LOOP
        v_uid := gen_random_uuid();
        v_pid := gen_random_uuid();
        INSERT INTO public.usuarios (id,email,emailconfirmado,passwordhash,nombreusuario,
            telefonoconfirmado,autenticaciondosfactores,bloqueohabilitado,
            contadoraccesosfallidos,fechacreacion,activo)
        VALUES (v_uid,'espectador'||i||'@demo.com',true,v_hash,'espectador'||i,
            false,false,true,0,v_now,true);
        INSERT INTO public.personas (id,usuarioid,nombre,apellidopaterno,fechacreacion,activo)
        VALUES (v_pid,v_uid,
            v_nombres[1 + ((i - 1) % 25)],
            v_apellidos[1 + ((i - 1) % 20)],
            v_now,true);
        INSERT INTO public.usuariosroles (usuarioid,rolid,fechaasignacion)
        VALUES (v_uid,v_rol_espectador,v_now);
    END LOOP;

    -- ── ÁRBITROS (8, todos aprobados) ──────────────────────────
    FOR i IN 1..8 LOOP
        v_uid := gen_random_uuid();
        v_pid := gen_random_uuid();
        INSERT INTO public.usuarios (id,email,emailconfirmado,passwordhash,nombreusuario,
            telefonoconfirmado,autenticaciondosfactores,bloqueohabilitado,
            contadoraccesosfallidos,fechacreacion,activo)
        VALUES (v_uid,'arbitro'||i||'@demo.com',true,v_hash,'arbitro'||i,
            false,false,true,0,v_now,true);
        INSERT INTO public.personas (id,usuarioid,nombre,apellidopaterno,fechacreacion,activo)
        VALUES (v_pid,v_uid,
            v_nombres[1 + ((i + 7) % 25)],
            v_apellidos[1 + ((i + 5) % 20)],
            v_now,true);
        INSERT INTO public.usuariosroles (usuarioid,rolid,fechaasignacion)
        VALUES (v_uid,v_rol_arbitro,v_now);
        INSERT INTO public.arbitros (personaid,numerolicencia,categoriasarbitra,estadoaprobacion,fechaaprobacion)
        VALUES (v_pid,
            'ARB-2025-' || LPAD(i::TEXT,4,'0'),
            'Amateur,Municipal,Regional',
            'Aprobado',
            v_now);
    END LOOP;

    -- ── ENTRENADORES + EQUIPOS (24 cada uno) ──────────────────
    FOR i IN 1..24 LOOP
        v_uid := gen_random_uuid();
        v_pid := gen_random_uuid();
        INSERT INTO public.usuarios (id,email,emailconfirmado,passwordhash,nombreusuario,
            telefonoconfirmado,autenticaciondosfactores,bloqueohabilitado,
            contadoraccesosfallidos,fechacreacion,activo)
        VALUES (v_uid,'entrenador'||i||'@demo.com',true,v_hash,'entrenador'||i,
            false,false,true,0,v_now,true);
        INSERT INTO public.personas (id,usuarioid,nombre,apellidopaterno,fechacreacion,activo)
        VALUES (v_pid,v_uid,
            v_nombres[1 + ((i - 1) % 25)],
            v_apellidos[1 + ((i + 3)  % 20)],
            v_now,true);
        INSERT INTO public.usuariosroles (usuarioid,rolid,fechaasignacion)
        VALUES (v_uid,v_rol_entrenador,v_now);

        INSERT INTO public.entrenadores (personaid,numerolicencia,especialidad)
        VALUES (v_pid,'LIC-ENT-'||LPAD(i::TEXT,4,'0'),'Táctica General')
        RETURNING id INTO v_eid;

        INSERT INTO public.equipos (nombre,abreviacion,sede,fechacreacion,activo)
        VALUES (v_equipos[i],v_abrevs[i],v_sedes_equipo[1+((i-1)%8)],v_now,true)
        RETURNING id INTO v_equipo_id;

        INSERT INTO public.equiposentrenadores (entrenadorid,equipoid,rol,fechainicio,espropietario,activo)
        VALUES (v_eid,v_equipo_id,'Principal',v_now,true,true);
    END LOOP;

    -- ── JUGADORES (10 por equipo = 240 total) ─────────────────
    FOR i IN 1..24 LOOP
        SELECT id INTO v_equipo_id FROM public.equipos WHERE nombre = v_equipos[i];
        FOR j IN 1..10 LOOP
            v_uid := gen_random_uuid();
            v_pid := gen_random_uuid();
            INSERT INTO public.usuarios (id,email,emailconfirmado,passwordhash,nombreusuario,
                telefonoconfirmado,autenticaciondosfactores,bloqueohabilitado,
                contadoraccesosfallidos,fechacreacion,activo)
            VALUES (v_uid,'jug_e'||i||'_'||j||'@demo.com',true,v_hash,'jug_e'||i||'_'||j,
                false,false,true,0,v_now,true);
            INSERT INTO public.personas (id,usuarioid,nombre,apellidopaterno,
                fechanacimiento,fechacreacion,activo)
            VALUES (v_pid,v_uid,
                v_nombres[1 + ((i * 10 + j) % 25)],
                v_apellidos[1 + ((i * 3  + j) % 20)],
                (v_now - INTERVAL '20 years' - (((j-1) % 10)::TEXT || ' years')::INTERVAL)::DATE,
                v_now,true);
            INSERT INTO public.usuariosroles (usuarioid,rolid,fechaasignacion)
            VALUES (v_uid,v_rol_jugador,v_now);
            INSERT INTO public.jugadores (personaid,posicionpreferida,numerocamiseta)
            VALUES (v_pid, v_posiciones[1+((j-1)%11)], j)
            RETURNING id INTO v_jid;
        END LOOP;
    END LOOP;

END $$;

-- ========================================================
-- PARTE 3: TORNEOS
-- ========================================================
-- Torneo 1: Finalizado 2024  (equipos 1-8,   Liga Nacional)
-- Torneo 2: Finalizado 2025  (equipos 9-16,  Liga Regional)
-- Torneo 3: Activo 2025-26   (equipos 1-8,   Liga Nacional)   ← en progreso, tiene fechas en Mar 2026
-- Torneo 4: Activo 2026      (equipos 9-16,  Copa Interligas) ← tiene fechas en Mar 2026
-- Torneo 5: Activo 2026 Juv  (equipos 17-24, Liga Regional)   ← empieza Mar 2026
-- Torneo 6: Borrador 2027    (equipos 1-8,   Liga Nacional)   ← sin partidos aún
INSERT INTO public.torneos
    (ligaid,temporadaid,categoriaid,nombre,formato,estado,
     fechainicio,fechafin,numeroequiposmaximo,numeroequiposminimo,fechacreacion)
SELECT
    l.id, t.id, c.id,
    nd.nombre, nd.formato, nd.estado,
    nd.finicio, nd.ffin, 8, 4, NOW()
FROM (VALUES
    ('Liga Nacional',   'Temporada 2024', 'Amateur',
     'Torneo Apertura 2024',      'TodosContraTodos', 'Finalizado',
     '2024-08-03'::DATE, '2024-12-14'::DATE),

    ('Liga Regional',   'Temporada 2025', 'Amateur',
     'Copa Regional 2025',        'TodosContraTodos', 'Finalizado',
     '2025-02-08'::DATE, '2025-05-31'::DATE),

    ('Liga Nacional',   'Temporada 2025', 'Amateur',
     'Liga Nacional 2025-26',     'TodosContraTodos', 'Activo',
     '2025-09-06'::DATE, '2026-06-30'::DATE),

    ('Copa Interligas', 'Temporada 2026', 'Amateur',
     'Copa Interligas 2026',      'TodosContraTodos', 'Activo',
     '2026-02-21'::DATE, '2026-08-31'::DATE),

    ('Liga Regional',   'Temporada 2026', 'Juvenil',
     'Liga Regional Juvenil 2026','TodosContraTodos', 'Activo',
     '2026-03-07'::DATE, '2026-11-29'::DATE),

    ('Liga Nacional',   'Temporada 2027', 'Amateur',
     'Torneo Apertura 2027',      'TodosContraTodos', 'Borrador',
     '2027-01-15'::DATE, '2027-07-12'::DATE)
) AS nd(ligaNombre,tempNombre,catNombre,nombre,formato,estado,finicio,ffin)
JOIN public.ligas      l ON l.nombre = nd.ligaNombre
JOIN public.temporadas t ON t.nombre = nd.tempNombre
JOIN public.categorias c ON c.nombre = nd.catNombre;

-- Configuraciones de todos los torneos
INSERT INTO public.configuracionestorneo
    (torneoid,puntosporvictoria,puntosporempate,puntosporderrota,
     criteriodesempate1,criteriodesempate2,
     tarjetasamarillasparasuspension,partidossuspensiontarjetaroja,
     minimojugadorestitulares)
SELECT id, 3, 1, 0, 'GolDiferencia', 'GolesAFavor', 5, 1, 11
FROM public.torneos;

-- ========================================================
-- PARTE 4: EQUIPOS POR TORNEO (8 equipos c/u)
-- ========================================================
DO $$
DECLARE
    v_now       TIMESTAMP := NOW();
    t_ids       INT[];
    e_ids       INT[];
    -- offset base en array de equipos para cada torneo (1-indexed)
    v_offsets   INT[] := ARRAY[0, 8, 0, 8, 16, 0];
    t_idx       INT;
    eq          INT;
    torneo_id   INT;
    equipo_id   INT;
BEGIN
    SELECT ARRAY(SELECT id FROM public.torneos ORDER BY id) INTO t_ids;
    SELECT ARRAY(SELECT id FROM public.equipos  ORDER BY id) INTO e_ids;

    FOR t_idx IN 1..6 LOOP
        torneo_id := t_ids[t_idx];
        FOR eq IN 1..8 LOOP
            equipo_id := e_ids[v_offsets[t_idx] + eq];
            INSERT INTO public.equipostorneos (equipoid,torneoid,fechainscripcion,estado)
            VALUES (equipo_id, torneo_id, v_now, 'Activo')
            ON CONFLICT DO NOTHING;
        END LOOP;
    END LOOP;
END $$;

-- ========================================================
-- PARTE 5: JUGADORES POR TORNEO (10 por equipo)
-- ========================================================
DO $$
DECLARE
    v_now       TIMESTAMP := NOW();
    t_ids       INT[];
    e_ids       INT[];
    v_offsets   INT[] := ARRAY[0, 8, 0, 8, 16, 0];
    t_idx       INT;
    eq          INT;
    equipo_idx  INT;
    torneo_id   INT;
    equipo_id   INT;
    jug_id      INT;
    cam         INT;
BEGIN
    SELECT ARRAY(SELECT id FROM public.torneos ORDER BY id) INTO t_ids;
    SELECT ARRAY(SELECT id FROM public.equipos  ORDER BY id) INTO e_ids;

    -- Solo torneos 1-5 (no Borrador)
    FOR t_idx IN 1..5 LOOP
        torneo_id := t_ids[t_idx];
        FOR eq IN 1..8 LOOP
            equipo_idx := v_offsets[t_idx] + eq;  -- 1-based index into equipos
            equipo_id  := e_ids[equipo_idx];
            cam := 0;
            FOR jug_id IN (
                SELECT j.id
                FROM public.jugadores j
                JOIN public.personas p  ON j.personaid = p.id
                JOIN public.usuarios  u ON p.usuarioid = u.id
                WHERE u.email LIKE 'jug_e' || equipo_idx || '_%'
                ORDER BY j.id
                LIMIT 10
            ) LOOP
                cam := cam + 1;
                INSERT INTO public.equiposjugadorestorneos
                    (jugadorid,equipoid,torneoid,numerocamiseta,fechainscripcion,activo)
                VALUES (jug_id, equipo_id, torneo_id, cam, v_now, true)
                ON CONFLICT DO NOTHING;
            END LOOP;
        END LOOP;
    END LOOP;
END $$;

-- ========================================================
-- PARTE 6: PARTIDOS (todos contra todos, 28 por torneo)
-- ========================================================
-- Distribución de fechas por torneo:
--   Torneo 1 (Finalizado 2024):    7-días entre rondas, inicio Aug 3 2024  → todas finalizadas
--   Torneo 2 (Finalizado 2025):    7-días entre rondas, inicio Feb 8 2025  → todas finalizadas
--   Torneo 3 (Liga Nac. Activa):  40-días entre rondas, inicio Sep 6 2025  → rondas 0-4 pasadas, 5-6 futuras (Mar/May 2026)
--   Torneo 4 (Copa 2026 Activa):   7-días entre rondas, inicio Feb 21 2026 → rondas 0-1 pasadas, 2-6 futuras (Mar 2026+)
--   Torneo 5 (Liga Juv. Activa):  14-días entre rondas, inicio Mar 7 2026  → todas futuras o hoy
-- Resultado: Marzo 2026 tiene matches de torneos 3, 4 y 5 simultáneamente.
DO $$
DECLARE
    v_now         TIMESTAMP := NOW();
    t_ids         INT[];
    e_ids         INT[];
    a_ids         INT[];
    v_offsets     INT[]  := ARRAY[0, 8, 0, 8, 16];
    v_starts      DATE[] := ARRAY[
        '2024-08-03'::DATE,   -- Torneo 1
        '2025-02-08'::DATE,   -- Torneo 2
        '2025-09-06'::DATE,   -- Torneo 3
        '2026-02-21'::DATE,   -- Torneo 4
        '2026-03-07'::DATE    -- Torneo 5
    ];
    -- Intervalo en días entre rondas por torneo
    v_intervals   INT[]  := ARRAY[7, 7, 40, 7, 14];
    -- Horas por posición-en-ronda: 4 partidos por ronda (Sab 10h, Sab 15h, Dom 10h, Dom 16h)
    v_horas       INT[]  := ARRAY[10, 15, 10, 16];
    v_day_offsets INT[]  := ARRAY[0,  0,  1,  1];

    v_sedes TEXT[] := ARRAY[
        'Estadio Nacional','Campo Norte','Cancha Central','Complejo Sur',
        'Estadio Municipal','Campo La Paz','Arena Deportiva','Cancha La Unidad'
    ];

    t_idx       INT;
    i           INT;
    j           INT;
    match_num   INT;
    round_num   INT;
    pos_ronda   INT;
    torneo_id   INT;
    e_local_id  INT;
    e_visit_id  INT;
    arb_id      INT;
    match_ts    TIMESTAMP;
    v_estado    TEXT;
    v_goles_l   INT;
    v_goles_v   INT;
    v_interval  INT;
    v_start     DATE;
    v_day_off   INT;
    v_hora      INT;
BEGIN
    SELECT ARRAY(SELECT id FROM public.torneos ORDER BY id LIMIT 5) INTO t_ids;
    SELECT ARRAY(SELECT id FROM public.equipos  ORDER BY id)        INTO e_ids;
    SELECT ARRAY(SELECT id FROM public.arbitros ORDER BY id)        INTO a_ids;

    FOR t_idx IN 1..5 LOOP
        torneo_id  := t_ids[t_idx];
        v_start    := v_starts[t_idx];
        v_interval := v_intervals[t_idx];
        match_num  := 0;

        -- Todos contra todos: C(8,2) = 28 partidos
        FOR i IN 1..7 LOOP
            FOR j IN (i+1)..8 LOOP
                e_local_id := e_ids[v_offsets[t_idx] + i];
                e_visit_id := e_ids[v_offsets[t_idx] + j];

                round_num  := match_num / 4;           -- ronda (0-6)
                pos_ronda  := match_num % 4;           -- posición dentro de ronda (0-3)
                v_hora     := v_horas[1 + pos_ronda];
                v_day_off  := v_day_offsets[1 + pos_ronda];

                match_ts := (v_start
                    + (round_num * v_interval + v_day_off)::INT * INTERVAL '1 day'
                    + v_hora::TEXT::INTERVAL)::TIMESTAMP;

                -- Estado según si la fecha es pasada o futura
                IF match_ts < v_now - INTERVAL '2 hours' THEN
                    v_estado  := 'Finalizado';
                    v_goles_l := (random() * 4)::INT;
                    v_goles_v := (random() * 4)::INT;
                ELSIF match_ts < v_now + INTERVAL '2 hours' THEN
                    v_estado  := 'EnCurso';
                    v_goles_l := NULL;
                    v_goles_v := NULL;
                ELSE
                    v_estado  := 'Programado';
                    v_goles_l := NULL;
                    v_goles_v := NULL;
                END IF;

                arb_id := a_ids[1 + (match_num % array_length(a_ids, 1))];

                INSERT INTO public.partidos
                    (torneoid, equipolocalid, equipovisitanteid, arbitroid,
                     fecha, sede, estado, goleslocal, golesvisitante, fechacreacion)
                VALUES (
                    torneo_id,
                    e_local_id,
                    e_visit_id,
                    arb_id,
                    match_ts,
                    v_sedes[1 + (match_num % 8)],
                    v_estado,
                    v_goles_l,
                    v_goles_v,
                    v_now
                );

                match_num := match_num + 1;
            END LOOP;
        END LOOP;
    END LOOP;
END $$;

-- ========================================================
-- VERIFICACIÓN FINAL
-- ========================================================
SELECT 'usuarios'               AS tabla, COUNT(*) AS total FROM public.usuarios       UNION ALL
SELECT 'personas',                         COUNT(*) FROM public.personas               UNION ALL
SELECT 'equipos',                          COUNT(*) FROM public.equipos                UNION ALL
SELECT 'jugadores',                        COUNT(*) FROM public.jugadores              UNION ALL
SELECT 'entrenadores',                     COUNT(*) FROM public.entrenadores           UNION ALL
SELECT 'arbitros',                         COUNT(*) FROM public.arbitros               UNION ALL
SELECT 'torneos',                          COUNT(*) FROM public.torneos                UNION ALL
SELECT 'equipostorneos',                   COUNT(*) FROM public.equipostorneos         UNION ALL
SELECT 'equiposjugadorestorneos',          COUNT(*) FROM public.equiposjugadorestorneos UNION ALL
SELECT 'equiposentrenadores',              COUNT(*) FROM public.equiposentrenadores    UNION ALL
SELECT 'partidos',                         COUNT(*) FROM public.partidos               UNION ALL
SELECT 'configuracionestorneo',            COUNT(*) FROM public.configuracionestorneo
ORDER BY tabla;

-- ================================================================
-- CÓMO EXPORTAR DESDE SUPABASE:
--
-- Opción A — Backup completo (recomendado para entrega):
--   Supabase Dashboard → Project Settings → Database → Backups
--   O usar pg_dump desde CLI:
--   pg_dump "postgresql://postgres:postgres@127.0.0.1:54322/postgres"
--     --schema=public --data-only --file=export_demo.sql
--
-- Opción B — Solo exportar datos (SQL inserts):
--   pg_dump "..." --schema=public --data-only
--     --inserts --column-inserts --file=export_inserts.sql
--
-- Opción C — Incluir estructura + datos (para DB nueva):
--   Primero ejecutar: dotnet ef migrations script > create_tables.sql
--   Luego ejecutar create_tables.sql en la DB nueva, después este seed.
-- ================================================================
