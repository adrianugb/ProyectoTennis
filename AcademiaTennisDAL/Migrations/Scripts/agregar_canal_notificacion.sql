-- Ejecutar este script en la base de datos MySQL local de cada integrante
-- después de hacer git pull, para sincronizar la tabla Notificaciones
-- con los nuevos campos CanalUsado y EnvioFallido (USER-09-008).

USE AcademiaTennisBD;

ALTER TABLE Notificaciones
  ADD COLUMN CanalUsado VARCHAR(50) NOT NULL DEFAULT 'Plataforma';

ALTER TABLE Notificaciones
  ADD COLUMN EnvioFallido TINYINT(1) NOT NULL DEFAULT 0;