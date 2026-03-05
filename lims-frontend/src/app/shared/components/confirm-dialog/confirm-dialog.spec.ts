import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ConfirmDialog } from './confirm-dialog';

describe('ConfirmDialog', () => {
    let component: ConfirmDialog;
    let fixture: ComponentFixture<ConfirmDialog>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [ConfirmDialog],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(ConfirmDialog);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have default isOpen as false', () => {
        expect(component.isOpen).toBeFalse();
    });

    it('should have default title as Confirm', () => {
        expect(component.title).toBe('Confirm');
    });

    it('should have default message', () => {
        expect(component.message).toBe('Are you sure?');
    });

    it('should emit confirmed event on confirm', () => {
        spyOn(component.confirmed, 'emit');
        component.onConfirm();
        expect(component.confirmed.emit).toHaveBeenCalled();
    });

    it('should emit cancelled event on cancel', () => {
        spyOn(component.cancelled, 'emit');
        component.onCancel();
        expect(component.cancelled.emit).toHaveBeenCalled();
    });

    it('should have default confirmText as Delete', () => {
        expect(component.confirmText).toBe('Delete');
    });
});
