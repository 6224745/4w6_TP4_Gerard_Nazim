import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  
  req = req.clone({
    setHeaders:{
<<<<<<< HEAD
=======
      'Content-Type' : 'application/json',
>>>>>>> origin/dev
      'Authorization' : 'Bearer ' + localStorage.getItem("token")
    }
  });
  
  return next(req);
};
