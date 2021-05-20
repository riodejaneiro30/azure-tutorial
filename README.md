# Azure Tutorial

Endpoint (Without Nexus) :
- Create book : http://func-siperpus.azurewebsites.net/api/Book [POST]
Request body :
{
    "title": "title101",
    "category": "category101"
}
- Get all book : http://func-siperpus.azurewebsites.net/api/Book [GET]
- Get book by id : http://func-siperpus.azurewebsites.net/api/Book/{id:guid} [GET]
- Update book : http://func-siperpus.azurewebsites.net/api/Book [PUT]
{"id": "id1"	
    "title": "title101",
    "category": "category101"
}
- Delete book : http://func-siperpus.azurewebsites.net/api/Book/{id:guid} [DELETE]

Endpoint (With Nexus) :
- Create book : http://func-siperpus.azurewebsites.net/api/BookNexus [POST]
Request body :
{
    "title": "title101",
    "category": "category101"
}
- Get all book : http://func-siperpus.azurewebsites.net/api/BookNexus [GET]
- Get book by id : http://func-siperpus.azurewebsites.net/api/BookNexus/{id:guid} [GET]
- Update book : http://func-siperpus.azurewebsites.net/api/BookNexus [PUT]
{"id" : "id1"
    "title": "title101",
    "category": "category101",
"code": "xxxx"
}
- Delete book : http://func-siperpus.azurewebsites.net/api/BookNexus/{id:guid} [DELETE]