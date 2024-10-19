import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);

  return next(req).pipe(
    catchError(error => {
      if (error) {
        switch (error.status) {
          case 400:
            const errors = error.error.errors;

            if (errors == null) {
              toastr.error(error.error, error.status);
              break;
            }

            const modelStateErrors = [];

              for (const key in errors) {
                if (errors[key]) {
                  modelStateErrors.push(errors[key]);
                }
              }

              throw modelStateErrors.flat();
          case 401:
            toastr.error('Unauthorized', error.status);
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navigationExtras: NavigationExtras = {state: {error: error.error}};
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          default:
            toastr.error('Something unexpected went wrong.');
            break;
        }
      }

      throw error;
    })
  );
};
