import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ErrorService } from '../services/';
import { ApiService } from '../services/api';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const errorService = inject(ErrorService);
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // These status codes should trigger the global error page
            const criticalStatuses = [0, 403, 404, 500, 503];

            // 401 is special, we might want to let the auth interceptor handle it or redirect to login
            // But for now, let's include it in global handling for simplicity if preferred
            const allHandledStatuses = [...criticalStatuses, 401];

            if (allHandledStatuses.includes(error.status)) {
                errorService.setError({
                    code: error.status,
                    message: error.statusText || 'Server Error',
                    description: ApiService.getErrorMessage(error),
                    type: errorService.mapStatusCodeToType(error.status)
                });

                router.navigate(['/error']);
            }

            return throwError(() => error);
        })
    );
};
