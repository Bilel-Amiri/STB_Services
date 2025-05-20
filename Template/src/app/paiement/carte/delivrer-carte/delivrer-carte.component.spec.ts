import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DelivrerCarteComponent } from './delivrer-carte.component';

describe('DelivrerCarteComponent', () => {
  let component: DelivrerCarteComponent;
  let fixture: ComponentFixture<DelivrerCarteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DelivrerCarteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DelivrerCarteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
