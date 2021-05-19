# Azure Tutorial

Endpoint :
- Create book : http://func-siperpus.azurewebsites.net/api/book [POST]
Request body :
{
    "title": "title101",
    "category": "category101"
}
- Get all book : http://func-siperpus.azurewebsites.net/api/book [GET]
- Get book by id : http://func-siperpus.azurewebsites.net/api/book/{id:guid} [GET]
- Update book : http://func-siperpus.azurewebsites.net/api/book/{id:guid} [PUT]
{
    "title": "title101",
    "category": "category101"
}
- Delete book : http://func-siperpus.azurewebsites.net/api/book/{id:guid} [DELETE]