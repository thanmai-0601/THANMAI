import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast';

describe('ToastService', () => {
    let service: ToastService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [ToastService]
        });
        service = TestBed.inject(ToastService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should have success method', () => {
        expect(service.success).toBeDefined();
    });

    it('should have error method', () => {
        expect(service.error).toBeDefined();
    });

    it('should have warning method', () => {
        expect(service.warning).toBeDefined();
    });

    it('should have info method', () => {
        expect(service.info).toBeDefined();
    });

    it('should have remove method', () => {
        expect(service.remove).toBeDefined();
    });
});
