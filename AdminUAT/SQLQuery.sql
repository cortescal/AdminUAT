select * from Denuncia
inner join MP on Denuncia.MPId = MP.Id
inner join UR on MP.URId = UR.Id
where Denuncia.Paso = 3 and Denuncia.BitaKioscoId = 1 and (Denuncia.AltaSistema between '2019-04-01' and '2019-04-06 23:59:59')
and UR.RegionId = 6