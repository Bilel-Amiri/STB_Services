import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CarteDetailsComponent } from './carte-details.component';

describe('CarteDetailsComponent', () => {
  let component: CarteDetailsComponent;
  let fixture: ComponentFixture<CarteDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CarteDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CarteDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
