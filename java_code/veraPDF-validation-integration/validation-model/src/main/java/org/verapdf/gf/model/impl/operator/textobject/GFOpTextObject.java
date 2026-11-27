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
package org.verapdf.gf.model.impl.operator.textobject;

import org.verapdf.cos.COSBase;
import org.verapdf.gf.model.impl.operator.base.GFOperator;
import org.verapdf.model.operator.OpTextObject;

import java.util.List;

/**
 * Shared validation-model wrapper for operators that delimit text objects.
 *
 * <p>The generic constructor uses the {@code OpTextObject} model type;
 * specialized operators can pass their own type through the second
 * constructor. The base class handles their operand list.</p>
 *
 * <p><b>.NET port:</b> Implement
 * {@code VeraPdf.Model.Operator.IOpTextObject} with the ported operator
 * base and a {@code List<T>} of parser COS values. Keep the model names
 * exact because validation profiles use them for dispatch.</p>
 *
 * @author Timur Kamalov
 */
public class GFOpTextObject extends GFOperator implements OpTextObject {

	/** Type name for {@code GFOpTextObject} */
    public static final String OP_TEXT_OBJECT_TYPE = "OpTextObject";

    public GFOpTextObject(List<COSBase> arguments) {
        this(arguments, OP_TEXT_OBJECT_TYPE);
    }

    public GFOpTextObject(List<COSBase> arguments, String type) {
        super(arguments, type);
    }

}
