/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.operator.color;

import org.verapdf.cos.COSBase;
import org.verapdf.cos.COSName;
import org.verapdf.cos.COSObjType;
import org.verapdf.gf.model.impl.cos.GFCosName;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosName;
import org.verapdf.model.operator.OpColor;
import org.verapdf.model.pdlayer.PDColorSpace;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Base for color-setting operators associated with a current color space. It exposes colorSpace and optional patternName links; a trailing COS name identifies a pattern rather than another numeric component.
 *
 * <p> It is an internal operator base type and it implements {@code OpColor}. Operator objects are created while parsing content streams and are queried by validation profiles through their typed links. Named links include {@code colorSpace}, {@code patternName}.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.Color.GFOpColor}, implementing {@code VeraPdf.Model.Operator.IOpColor}. Expose links as {@code IReadOnlyList<IModelObject>} from {@code GetLinkedObjects(string)}; preserve nullable or empty links and map parser COS operands to {@code VeraPdf.Parser.Cos.CosValue} types. Java read-only {@code List} results should remain read-only in C#.</p>
 *
 * @author Timur Kamalov
 */
public class GFOpColor extends GFOpSetColor implements OpColor {

	/** Type name for {@code GFOpColor} */
    public static final String OP_COLOR_TYPE = "OpColor";
    public static final String COLOR_SPACE = "colorSpace";
    public static final String PATTERN_NAME = "patternName";

    private final PDColorSpace colorSpace;

    public GFOpColor(List<COSBase> arguments, PDColorSpace colorSpace, String type) {
        super(arguments, type);
        this.colorSpace = colorSpace;
    }

    public GFOpColor(List<COSBase> arguments, PDColorSpace colorSpace) {
        this(arguments, colorSpace, OP_COLOR_TYPE);
    }

    @Override
    public List<? extends Object> getLinkedObjects(String link) {
        switch (link) {
            case COLOR_SPACE:
                return getColorSpace();
            case PATTERN_NAME:
                return getPatternName();
            default:
                return super.getLinkedObjects(link);
        }
    }

    private List<CosName> getPatternName() {
        int size = this.arguments.size();
        if (size > 0) {
            COSBase cosBase = this.arguments.get(size - 1);
            if (cosBase.getType() == COSObjType.COS_NAME) {
                List<CosName> res = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
                res.add(new GFCosName((COSName) cosBase));
                return Collections.unmodifiableList(res);
            }
        }
        return Collections.emptyList();
    }

    private List<PDColorSpace> getColorSpace() {
        if (this.colorSpace != null) {
            List<PDColorSpace> res = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            res.add(colorSpace);
            return Collections.unmodifiableList(res);
        }
        return Collections.emptyList();
    }
}
