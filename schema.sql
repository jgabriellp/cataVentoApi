-- public."Group" definição

-- Drop table

-- DROP TABLE public."Group";

CREATE TABLE public."Group" (
	"GroupId" serial4 NOT NULL,
	"GroupName" varchar(100) NOT NULL,
	CONSTRAINT "Group_GroupName_key" UNIQUE ("GroupName"),
	CONSTRAINT "Group_pkey" PRIMARY KEY ("GroupId")
);


-- public."Usuario" definição

-- Drop table

-- DROP TABLE public."Usuario";

CREATE TABLE public."Usuario" (
	"Id" bigserial NOT NULL,
	"Name" varchar(100) NOT NULL,
	"LastName" varchar(100) NOT NULL,
	"Role" int2 NOT NULL,
	"Email" varchar(255) NOT NULL,
	"Password" text NOT NULL,
	"PhotoUrl" varchar(500) NULL,
	"IsDeleted" bool DEFAULT false NOT NULL,
	CONSTRAINT "Usuario_Email_key" UNIQUE ("Email"),
	CONSTRAINT "Usuario_pkey" PRIMARY KEY ("Id")
);


-- public."Notice" definição

-- Drop table

-- DROP TABLE public."Notice";

CREATE TABLE public."Notice" (
	"NoticeId" bigserial NOT NULL,
	"Title" varchar(100) NOT NULL,
	"Content" text NOT NULL,
	"IsActive" bool DEFAULT true NOT NULL,
	"DateCreated" timestamptz DEFAULT now() NOT NULL,
	"PhotoUrl" varchar(500) NULL,
	"CreatorId" int8 NOT NULL,
	CONSTRAINT "Notice_pkey" PRIMARY KEY ("NoticeId"),
	CONSTRAINT fk_notice_creator FOREIGN KEY ("CreatorId") REFERENCES public."Usuario"("Id")
);


-- public."NoticeAudience" definição

-- Drop table

-- DROP TABLE public."NoticeAudience";

CREATE TABLE public."NoticeAudience" (
	"NoticeAudienceId" bigserial NOT NULL,
	"NoticeId" int8 NOT NULL,
	"AudienceRole" int2 NOT NULL,
	CONSTRAINT "NoticeAudience_pkey" PRIMARY KEY ("NoticeAudienceId"),
	CONSTRAINT uq_noticeaudience_role UNIQUE ("NoticeId", "AudienceRole"),
	CONSTRAINT fk_noticeaudience_notice FOREIGN KEY ("NoticeId") REFERENCES public."Notice"("NoticeId") ON DELETE CASCADE
);


-- public."Post" definição

-- Drop table

-- DROP TABLE public."Post";

CREATE TABLE public."Post" (
	"PostId" bigserial NOT NULL,
	"Content" text NOT NULL,
	"Date" timestamp DEFAULT now() NOT NULL,
	"ImageUrl" varchar(500) NULL,
	"GroupId" int4 NOT NULL,
	"CreatorId" int8 NOT NULL,
	CONSTRAINT "Post_pkey" PRIMARY KEY ("PostId"),
	CONSTRAINT fk_post_creator FOREIGN KEY ("CreatorId") REFERENCES public."Usuario"("Id"),
	CONSTRAINT fk_post_group FOREIGN KEY ("GroupId") REFERENCES public."Group"("GroupId") ON DELETE CASCADE
);


-- public."PostLiker" definição

-- Drop table

-- DROP TABLE public."PostLiker";

CREATE TABLE public."PostLiker" (
	"PostId" int8 NOT NULL,
	"UsuarioId" int8 NOT NULL,
	CONSTRAINT pk_postliker PRIMARY KEY ("PostId", "UsuarioId"),
	CONSTRAINT fk_postliker_post FOREIGN KEY ("PostId") REFERENCES public."Post"("PostId") ON DELETE CASCADE,
	CONSTRAINT fk_postliker_usuario FOREIGN KEY ("UsuarioId") REFERENCES public."Usuario"("Id")
);


-- public."UsuarioGroup" definição

-- Drop table

-- DROP TABLE public."UsuarioGroup";

CREATE TABLE public."UsuarioGroup" (
	"UsuarioId" int8 NOT NULL,
	"GroupId" int4 NOT NULL,
	CONSTRAINT pk_usuariogroup PRIMARY KEY ("UsuarioId", "GroupId"),
	CONSTRAINT fk_usuariogroup_group FOREIGN KEY ("GroupId") REFERENCES public."Group"("GroupId") ON DELETE CASCADE,
	CONSTRAINT fk_usuariogroup_usuario FOREIGN KEY ("UsuarioId") REFERENCES public."Usuario"("Id") ON DELETE CASCADE
);


-- public.tasks definição

-- Drop table

-- DROP TABLE public.tasks;

CREATE TABLE public.tasks (
	id serial4 NOT NULL,
	title varchar(255) NOT NULL,
	description text NULL,
	usuario_id int8 NULL,
	due_date timestamp NULL,
	priority int4 DEFAULT 1 NOT NULL,
	status int4 DEFAULT 1 NOT NULL,
	"position" int4 DEFAULT 0 NOT NULL,
	created_at timestamp DEFAULT CURRENT_TIMESTAMP NULL,
	board_type int4 DEFAULT 2 NOT NULL,
	CONSTRAINT tasks_pkey PRIMARY KEY (id),
	CONSTRAINT fk_usuario_tarefa FOREIGN KEY (usuario_id) REFERENCES public."Usuario"("Id") ON DELETE SET NULL
);
CREATE INDEX idx_tasks_board_type ON public.tasks USING btree (board_type, status, "position");
CREATE INDEX idx_tasks_status_position ON public.tasks USING btree (status, "position");


-- public."Comment" definição

-- Drop table

-- DROP TABLE public."Comment";

CREATE TABLE public."Comment" (
	"CommentId" bigserial NOT NULL,
	"Content" text NOT NULL,
	"Date" timestamp DEFAULT now() NOT NULL,
	"PostId" int8 NOT NULL,
	"CreatorId" int8 NOT NULL,
	CONSTRAINT "Comment_pkey" PRIMARY KEY ("CommentId"),
	CONSTRAINT fk_comment_creator FOREIGN KEY ("CreatorId") REFERENCES public."Usuario"("Id"),
	CONSTRAINT fk_comment_post FOREIGN KEY ("PostId") REFERENCES public."Post"("PostId") ON DELETE CASCADE
);