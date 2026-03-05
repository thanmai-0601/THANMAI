import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NotificationService } from './notification';
import { ApiService } from './api';

describe('NotificationService', () => {
    let service: NotificationService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [NotificationService, ApiService]
        });
        service = TestBed.inject(NotificationService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should have getMyNotifications method', () => {
        expect(service.getMyNotifications).toBeDefined();
    });

    it('should have markAsRead method', () => {
        expect(service.markAsRead).toBeDefined();
    });

    it('should be injectable', () => {
        expect(service instanceof NotificationService).toBeTrue();
    });

    it('should return an observable from getMyNotifications', () => {
        const result = service.getMyNotifications();
        expect(result).toBeDefined();
        expect(result.subscribe).toBeDefined();
    });
});
