# YOLO ONNX Model Specification for TRINETRA KAVACH

## Target Location:
Assets/Models/YOLO/yolo.onnx

## Supported Classes:
1. person (ID: 0)
2. fire_extinguisher (ID: 1)
3. electrical_panel (ID: 2)
4. exit (ID: 3)
5. gas_cylinder (ID: 4)
6. machine (ID: 5)
7. helmet (ID: 6)
8. gloves (ID: 7)
9. safety_shoes (ID: 8)
10. goggles (ID: 9)

## Inference Engine:
- Unity Sentis 1.3+
- Input Tensor: 1 x 3 x 640 x 640 (RGB normalized [0, 1])
- Output Tensor: 1 x 14 x 8400 (Bounding box xywh + 10 class scores)
- Development Fallback: `MockObjectDetector.cs` in `Assets/Scripts/AI/AISystems.cs` provides deterministic testing when `yolo.onnx` is not present.