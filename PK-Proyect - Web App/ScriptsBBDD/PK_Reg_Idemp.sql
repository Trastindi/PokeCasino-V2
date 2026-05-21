Create database PK_Reg;
use PK_Reg;


drop table if exists users;

create table if not exists users(id_user int primary key auto_increment,
					
                    nombre varchar(100) not null,
					apellido_1 varchar(100) not null,
					apellido_2 varchar(100),
					email varchar(100) not null unique,
                    username varchar(100) not null,
                    password varchar(100) not null
					);
                    
insert into users(nombre,apellido_1, email, username, password)
			values('admin', 'ad', 'admin@PKReg.es', 'admin', '1234');

insert into users(nombre,apellido_1, email, username, password)
			values('Red', 'rojo', 'Red@PKReg.es', 'Red', 'rojo');

insert into users(nombre,apellido_1, email, username, password)
			values('blue', 'azul', 'Blue@PKReg.es', 'Blue', 'azul');


            
            
Select * from users;