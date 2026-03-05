import { ErrorHandler, Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    constructor(private router: Router, private zone: NgZone) { }

    handleError(error: any): void {
        // Log the error to the console for debugging
        console.error('An unexpected error occurred:', error);

        // Ensure navigation runs inside the Angular zone
        this.zone.run(() => {
            this.router.navigate(['/error']);
        });
    }
}
