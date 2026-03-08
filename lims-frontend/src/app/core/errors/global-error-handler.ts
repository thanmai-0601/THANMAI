import { ErrorHandler, Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { ErrorService } from '../services/error';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    constructor(
        private router: Router,
        private zone: NgZone,
        private errorService: ErrorService
    ) { }

    handleError(error: any): void {
        // Log to console for dev awareness
        console.error('Core Logic Exception:', error);

        // Store error details
        this.errorService.setError({
            code: 500,
            message: 'Application Crash',
            description: error.message || 'An unexpected logic error occurred in the browser.',
            type: 'server'
        });

        // Ensure navigation runs inside the Angular zone
        this.zone.run(() => {
            this.router.navigate(['/error']);
        });
    }
}
