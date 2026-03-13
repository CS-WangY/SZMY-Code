
alter table ApprovalTemplateField add SelectOptionJson text not null default '' 
go
update ApprovalTemplateField set SelectOptionJson = '[
    {
        "key":"option-1638516844932",
        "value":[
            {
                "text":"主动备库"
            }
        ]
    },
    {
        "key":"option-1638516844933",
        "value":[
            {
                "text":"起订量要求"
            }
        ]
    }
]' where TemplateId = 'C4NvshbcGa1cGNUDrqqmhKw5z4Qq6au3SpZp8Vq4N' and FieldName = '备库类型'
